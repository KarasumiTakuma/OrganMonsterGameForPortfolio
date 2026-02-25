using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// バトル全体の進行管理を担当するクラス。
/// ターン制御、カード使用処理、バトル終了判定、UI更新などを統括する。
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Managers")]
    /// <summary>デッキ制御用マネージャ</summary>
    [SerializeField] private DeckManager deckManager;

    /// <summary>マナ制御用マネージャ</summary>
    [SerializeField] private ManaManager manaManager;

    /// <summary>手札UI制御用マネージャ</summary>
    [SerializeField] private HandAreaManager handAreaManager;

    /// <summary>敵エリア制御用マネージャ</summary>
    [SerializeField] private EnemyAreaManager enemyAreaManager;

    /// <summary>味方エリア制御用マネージャ</summary>
    [SerializeField] private AllyAreaManager allyAreaManager;

    /// <summary>プレイヤーターン終了ボタン</summary>
    [SerializeField] private GameObject endTurnButton;

    /// <summary>攻撃効果音</summary>
    [SerializeField] private AudioClip attackSoundEffect;

    /// <summary>回復効果音</summary>
    [SerializeField] private AudioClip healSoundEffect;

    /// <summary>バトル通知表示マネージャ</summary>
    [SerializeField] private BattleNoticeManager battleNoticeManager;

    /// <summary>Fireball生成マネージャ</summary>
    [SerializeField] private FireballManager fireballManager;

    [Header("UI")]

    /// <summary>フェーズ表示UI</summary>
    [SerializeField] private TMP_Text stagePhaseUI;

    /// <summary>最初のターンであるかどうかのフラグ</summary>
    private bool isFirstTurn = true;

    /// <summary>現在のバトル状態</summary>
    private BattleState battleState;

    /// <summary>終了処理の多重実行を防止するフラグ</summary>
    private bool isEndingBattle = false;

    /// <summary>ターンの切り替えにおいて、古いコルーチンを無効化するための番号</summary>
    private int turnSequenceID = 0;


    /// <summary>
    /// シーン開始前の初期化処理。
    /// デッキ生成・敵/味方データ設定などを行う。
    /// </summary>
    void Awake()
    {

        var battleSessionData = BattleSessionData.Instance;
        if (battleSessionData == null)
        {
            Debug.LogError("BattleSessionData が見つかりません！");
            return;
        }

        var currentStage = battleSessionData.GetCurrentStage();
        StagePhase firstPhase =
            currentStage.GetStagePhases()[battleSessionData.GetCurrentPhaseIndex()];

        // 敵・味方データ設定
        enemyAreaManager.SetEnemyData(firstPhase.GetEnemiesList());
        allyAreaManager.SetAllyData(battleSessionData.GetPlayerAlliesList().ConvertAll(ally => (MonsterData)ally));

        // デッキ構築
        deckManager.ClearDeck();
        foreach (var cardData in battleSessionData.GetPlayerDeckList())
        {
            deckManager.AddCardToDeck(cardData);
        }

        // デッキが作成されたら、山札をシャッフルする
        deckManager.ShuffleDeck();
    }

    /// <summary>
    /// ゲーム開始時の処理。
    /// オブジェクト生成・初期ドロー・イベント登録などを行う。
    /// </summary>
    void Start()
    {

        enemyAreaManager.SpawnEnemies();
        allyAreaManager.SpawnAllies();

        // ゲーム開始時の手札5枚ドロー。ゲーム開始時の手札は5枚。
        deckManager.DrawInitialHand();

        // Fireballのイベント登録
        fireballManager.OnFireballEffectTriggered += HandleFireballEffect;

        UpdatePhaseUI();

        StartCoroutine(BattleStartRoutine());

    }

    /// <summary>
    /// バトル開始演出用コルーチン。
    /// </summary>
    private IEnumerator BattleStartRoutine()
    {
        battleState = BattleState.TurnTransition;
        yield return battleNoticeManager.Show(BattleNoticeType.BattleStart);
        Log("バトル開始！", BattleLogType.System);
        StartPlayerTurn();
    }

    /// <summary>
    /// プレイヤーターン開始処理。
    /// </summary>
    private void StartPlayerTurn()
    {
        turnSequenceID++;
        StartCoroutine(PlayerTurnRoutine(turnSequenceID));
    }

    /// <summary>
    /// プレイヤーターン進行コルーチン。
    /// </summary>
    /// <param name="sequenceID">コルーチン識別ID</param>
    private IEnumerator PlayerTurnRoutine(int sequenceID)
    {
        
        battleState = BattleState.TurnTransition;

        yield return battleNoticeManager.Show(BattleNoticeType.PlayerTurn);

        Log("プレイヤーのターン開始！", BattleLogType.Attention);

        // 継続回復の処理
        allyAreaManager.ProcessHealOverTime();
        yield return new WaitForSeconds(1.0f);

        battleState = BattleState.PlayerTurn;

        manaManager.StartTurn();
        DrawCardAtTurnStart(); // ターン開始時に手札が5枚になるまでカードをドローする
        isFirstTurn = false;  // 1ターン目が終了すると、それ以降はフラグがfalseに。

        RefreshHandUI();

        handAreaManager.SetVisible(true);
        handAreaManager.SetInteractable(true);
        endTurnButton.SetActive(true);
        fireballManager.StartSpawning();
    }

    /// <summary>
    /// カードを使用して、効果を発動する処理。
    /// </summary>
    /// <param name="card">使用するカード</param>
    /// <param name="dropPosition">ドロップ位置</param>
    /// <param name="isSuccessCallback">使用成功通知コールバック</param>
    public void PlayCard(Card card, Vector2 dropPosition, System.Action<bool> isSuccessCallback)
    {
        if (battleState != BattleState.PlayerTurn)
        {
            isSuccessCallback?.Invoke(false);
            return;
        }

        // マナが足りるか確認
        if (!manaManager.UseMana(card.GetManaCost()))
        {
            Log("マナが足りません！", BattleLogType.Attention);
            isSuccessCallback?.Invoke(false); // カードを使用できなかったことを通知して、その時の処理を行う。
            return;
        }

        bool playSuccess = SuccessResolveCardAndEffect(card, dropPosition);

        if (playSuccess)
        {
            deckManager.DiscardCard(card);
            RefreshHandUI();
        }
        // カードを使用できたことを(isSuccessCallbackに登録しているメソッドに)通知して、その時の処理を行う。
        isSuccessCallback?.Invoke(playSuccess);

        StartCoroutine(CheckBattleEnd());
    }

    /// <summary>
    /// カードの効果タイプに応じて処理を分岐し、実際の効果を解決する。
    /// ダメージ・回復・継続効果(DOT/HOT)の適用を担当する。
    /// </summary>
    /// <param name="card"> 使用されたカードデータ。</param>
    /// <param name="dropPosition">
    /// カードをドロップした位置。スクリーン座標。
    /// 対象選択型カードのターゲット判定に使用する。
    /// </param>
    /// <returns> カード効果の適用に成功したかどうか。</returns>
    private bool SuccessResolveCardAndEffect(Card card, Vector2 dropPosition)
    {
        bool playSuccess = false;
        int targetIndex = -1;

        switch (card.GetCardEffectType())
        {
            case CardEffectType.AttackToSelected:
                targetIndex = enemyAreaManager.GetSelectedEnemyIndex(dropPosition);
                enemyAreaManager.TakeDamageToTargetEnemy(targetIndex, card.GetPower());
                playSuccess = true;
                AudioManager.Instance.PlaySE(attackSoundEffect);
                break;

            case CardEffectType.AttackToAll:
                enemyAreaManager.TakeDamageToAll(card.GetPower());
                AudioManager.Instance.PlaySE(attackSoundEffect);
                playSuccess = true;
                break;

            case CardEffectType.Heal:
                allyAreaManager.HealSharedHP(card.GetPower());
                AudioManager.Instance.PlaySE(healSoundEffect);
                playSuccess = true;
                break;

            case CardEffectType.DamageOverTime:
                targetIndex = enemyAreaManager.GetSelectedEnemyIndex(dropPosition);
                enemyAreaManager.ApplyDamageOverTimeToTargetEnemy(targetIndex, card.GetPower(), card.GetDurationTurn());
                playSuccess = true;
                break;

            case CardEffectType.HealOverTime:
                allyAreaManager.ApplyHealOverTime(card.GetPower(), card.GetDurationTurn());
                playSuccess = true;
                break;
        }

        return playSuccess;
    }

    /// <summary>
    /// プレイヤーターンを終了し、敵ターンへ移行する。
    /// プレイヤー操作UIの停止・演出停止処理を行う。
    /// </summary>
    public void EndPlayerTurn()
    {
        if (battleState != BattleState.PlayerTurn) return;

        // UI・演出停止
        endTurnButton.SetActive(false);
        handAreaManager.SetInteractable(false);
        fireballManager.StopSpawning();

        // 敵ターン開始
        StartEnemyTurn();
    }

    /// <summary>
    /// 敵ターン開始処理。
    /// ターン識別IDを更新し、敵ターンコルーチンを起動する。
    /// </summary>
    private void StartEnemyTurn()
    {
        turnSequenceID++;
        StartCoroutine(EnemyTurnRoutine(turnSequenceID));
    }

    /// <summary>
    /// 敵ターン進行コルーチン。
    /// 継続ダメージ処理 → 攻撃処理 → 終了判定を順に実行する。
    /// </summary>
    /// <param name="sequenceID">
    /// コルーチン識別用ID。
    /// ターン切替時に古い処理を無効化するために使用。
    /// </param>
    private IEnumerator EnemyTurnRoutine(int sequenceID)
    {
        battleState = BattleState.TurnTransition;

        yield return battleNoticeManager.Show(BattleNoticeType.EnemyTurn);
        Log("敵のターン!", BattleLogType.Attention);

        // 継続ダメージ処理
        enemyAreaManager.ProcessDamageOverTime();
        yield return new WaitForSeconds(1.0f);

        // 継続ダメージにより、敵が全滅し、バトルが終了したかをチェック
        yield return StartCoroutine(CheckBattleEnd());
        if (sequenceID != turnSequenceID) yield break;  // ターン更新済みなら処理を中断

        yield return new WaitForSeconds(0.8f);

        battleState = BattleState.EnemyTurn;  // その後、敵ターン状態にする

        // 攻撃ダメージ計算
        enemyAreaManager.PrepareEnemyAttackDamages();

        // 攻撃実行
        yield return StartCoroutine(AttackToAllySharedHP());

        // 再度、バトルが終了していないかを判定
        yield return StartCoroutine(CheckBattleEnd());
        if (battleState == BattleState.GameOver) yield break;
        if (sequenceID != turnSequenceID) yield break;

        // 次のプレイヤーターンへ
        StartPlayerTurn();
    }

    /// <summary>
    /// 敵の攻撃処理コルーチン。
    /// 各敵のダメージを順番に共有HPへ適用する。
    /// </summary>
    private IEnumerator AttackToAllySharedHP()
    {
        // 攻撃情報の取得
        IReadOnlyList<int> enemyAttackList = enemyAreaManager.GetEnemyAttackDamages();
        IReadOnlyList<string> aliveEnemyNamesList = enemyAreaManager.GetAliveEnemyNamesList();

        for (int i = 0; i < enemyAttackList.Count; i++)
        {
            if (i >= aliveEnemyNamesList.Count) break;

            string attackerName = aliveEnemyNamesList[i];
            int power = enemyAttackList[i];

            Log($"{attackerName}の攻撃！", BattleLogType.Attack);
            yield return new WaitForSeconds(1.0f);

            // ダメージ適用
            allyAreaManager.TakeDamageToSharedHP(power);
            AudioManager.Instance.PlaySE(attackSoundEffect);

            yield return new WaitForSeconds(1.0f);
        }
    }

    /// <summary>
    /// 手札UI更新処理。
    /// 手札データの同期およびUI再構築を行う。
    /// </summary>
    private void RefreshHandUI()
    {
        // deckManagerによってセットされた手札データを入手し、handAreaManagerにセット
        handAreaManager.setHandCardData(deckManager.GetHand());

        // 手札UI再構築, カード使用イベント登録
        handAreaManager.UpdateHandUI((card, dropPos, callback) => PlayCard(card, dropPos, callback), enemyAreaManager, allyAreaManager);
    }

    /// <summary>
    /// ターン開始時のドロー処理。
    /// 初ターン以外で手札枚数が上限未満の場合のみドローする。
    /// </summary>
    private void DrawCardAtTurnStart()
    {
        if (!isFirstTurn && deckManager.GetHandCount() < 5)
        {
            deckManager.DrawCardFull();
        }
    }

    /// <summary>
    /// バトル終了判定コルーチン。
    /// 勝利・敗北状態を検出し、遷移処理を実行する。
    /// </summary>
    private IEnumerator CheckBattleEnd()
    {
        // 多重実行防止
        if (isEndingBattle) yield break;

        // 敵が全滅していたら、「勝利」として処理する
        if (enemyAreaManager.GetIsAllMonstersDead())
        {
            isEndingBattle = true;
            Log("敵は全滅した！", BattleLogType.Attention);

            // プレイヤー操作・UI停止
            handAreaManager.SetVisible(false);
            endTurnButton.SetActive(false);
            fireballManager.StopSpawning();

            // 現在のステージ／フェーズ情報取得
            var battleSessionData = BattleSessionData.Instance;
            var currentStage = battleSessionData.GetCurrentStage();

            if (currentStage != null)
            {
                // 現在フェーズの取得
                int currentPhaseIndex = battleSessionData.GetCurrentPhaseIndex();
                StagePhase currentPhase = currentStage.GetStagePhases()[currentPhaseIndex];

                // 報酬ポイント取得
                int clearRewardPoints = currentPhase.GetClearRewardPoints();

                // 勝利演出＋報酬表示
                yield return battleNoticeManager.ShowVictoryAndReward(clearRewardPoints);

                // プレイヤーデータ更新
                GameManager.Instance.PlayerData.AddPoints(clearRewardPoints);

                // 次フェーズの有無で分岐
                if (battleSessionData.IsThereNextPhase())
                {
                    yield return StartCoroutine(StartNextPhaseRoutine());
                }
                else
                {
                    // 最終フェーズのみステージクリア
                    Log("ステージクリア！", BattleLogType.Attention);

                    yield return battleNoticeManager.Show(BattleNoticeType.StageClear);

                    // ステージクリアを記録
                    GameManager.Instance.PlayerData.SetClearedStage(currentStage.GetStageID());

                    yield return StartCoroutine(ReturnToLabScene());
                }
            }
        }

        // 味方(プレイヤー)が死んでいたら、「GameOver」として処理する
        if (allyAreaManager.GetIsPlayerDead())
        {
            isEndingBattle = true;
            battleState = BattleState.GameOver;

            Log("味方は全滅した…", BattleLogType.Attention);
            yield return StartCoroutine(battleNoticeManager.Show(BattleNoticeType.GameOver));// 敗北した際の通知の表示

            // UI・処理停止
            handAreaManager.SetVisible(false);
            endTurnButton.SetActive(false);
            fireballManager.StopSpawning();

            StartCoroutine(ReturnToLabScene());
        }
    }


    /// <summary>
    /// 次フェーズへ遷移するためのコルーチン。
    /// 演出 → フェーズ更新 → 敵再生成 → 状態初期化 の順で処理する。
    /// </summary>
    private IEnumerator StartNextPhaseRoutine()
    {
        battleState = BattleState.TurnTransition;
        Log("次のフェーズへ！", BattleLogType.System);

        // フェーズ遷移演出
        yield return StartCoroutine(battleNoticeManager.Show(BattleNoticeType.NextPhase));

        var battleSessionData = BattleSessionData.Instance;
        battleSessionData.AdvancePhase(); // フェーズインデックス更新(一つ進める)

        var currentStage = battleSessionData.GetCurrentStage();

        StagePhase nextPhase = currentStage.GetStagePhases()[battleSessionData.GetCurrentPhaseIndex()];

        // 前フェーズから持ち越される継続ダメージ(DOT)を削除
        enemyAreaManager.ClearAllDamageOverTime();

        // 次フェーズの敵データを設定
        enemyAreaManager.SetEnemyData(nextPhase.GetEnemiesList());

        // 敵を再スポーン
        enemyAreaManager.SpawnEnemies();

        // マナ初期化（フェーズ開始時の値へ）
        manaManager.ResetMana();

        UpdatePhaseUI();

        isEndingBattle = false;

        yield return new WaitForSeconds(1.5f);

        StartPlayerTurn();
    }

    /// <summary>
    /// 一定時間待機後、ラボシーンへ戻るコルーチン。
    /// </summary>
    private IEnumerator ReturnToLabScene()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.GoToLab();
    }



    /// <summary>
    /// 戦闘ログへメッセージを追加する共通処理。
    /// </summary>
    /// <param name="message">表示するログメッセージ</param>
    /// <param name="type">ログ種別（攻撃・回復・注意など）</param>
    private void Log(string message, BattleLogType type)
    {
        // シングルトンインスタンスであるBattleLogManagerインスタンスに追加したいログを送る
        BattleLogManager.Instance.AddLog(message, type);
        Debug.Log(message);  // デバッグログとしても表示する
    }

    /// <summary>
    /// フェーズUI表示を更新する。
    /// 「現在フェーズ / 総フェーズ数」の形式で表示。
    /// </summary>
    private void UpdatePhaseUI()
    {
        var battleSessionData = BattleSessionData.Instance;
        var currentStage = battleSessionData.GetCurrentStage();
        stagePhaseUI.text =
            battleSessionData.GetCurrentPhaseIndex() + 1 + " / " + currentStage.GetStagePhases().Count;
    }


    /// <summary>
    /// Fireballクリック時の効果処理。
    /// 効果タイプに応じてダメージ・回復を適用する。
    /// </summary>
    /// <param name="effectResult"> Fireballの効果内容（効果種別・効果量を保持）</param>
    private void HandleFireballEffect(FireballEffectResult effectResult)
    {
         // プレイヤーターン以外なら、Fireball効果は無効
        if (battleState != BattleState.PlayerTurn)
        {
            Debug.Log("プレイヤーターン以外なのでFireball効果は無効");
            return;
        }

        Debug.Log($"Fireball効果: {effectResult.effectType}, 効果量: {effectResult.effectAmount}");

        // 決定されたFireballのタイプ別にその効果を発動する
        switch (effectResult.effectType)
        {
            case FireballEffectType.DamageToAllEnemy:  // 敵全員に攻撃を与える効果
                enemyAreaManager.TakeDamageToAll(effectResult.effectAmount);
                break;
            case FireballEffectType.DamageToAlly:  // 味方(プレイヤー)に攻撃を与える効果
                allyAreaManager.TakeDamageToSharedHP(effectResult.effectAmount);
                break;
            case FireballEffectType.HealToAlly:        // 味方(プレイヤー)のHPを回復する効果
                allyAreaManager.HealSharedHP(effectResult.effectAmount);
                break;
        }

        // 効果適用後の終了判定
        StartCoroutine(CheckBattleEnd());
    }
}

/// <summary>
/// バトル進行状態を表す列挙型。
/// </summary>
public enum BattleState
{
    PlayerTurn,  // プレイヤーの手番
    EnemyTurn,   // 敵の手番
    TurnTransition,  // 演出中の状態
    Victory,        // 勝利状態
    GameOver        // 敗北状態
}
