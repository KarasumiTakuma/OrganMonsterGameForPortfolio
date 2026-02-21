using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private HandAreaManager handAreaManager; // HandAreaManagerの参照
    [SerializeField] private EnemyAreaManager enemyAreaManager;
    [SerializeField] private AllyAreaManager allyAreaManager;
    [SerializeField] private GameObject endTurnButton;  // プレイヤーターンの終了用ボタン
    [SerializeField] private AudioClip AttackSoundEffect;  // 攻撃の効果音
    [SerializeField] private AudioClip HealSoundEffect; // 回復の効果音
    [SerializeField] private BattleNoticeManager battleNoticeManager;  // バトル中の通知の表示管理用
    [SerializeField] private FireballManager fireballManager;  // Fireballの生成を 制御(実行/停止)

    [Header("UI")]
    [SerializeField] private TMP_Text stagePhaseUI;       // ステージのフェーズ表示用
    private bool isFirstTurn = true; // 最初のターンであるかどうかのフラグ。

    private BattleState battleState; // バトル中の状態をBattleState型として保持するための変数
    private bool isEndingBattle = false;  // CheckBattleEndが多重実行されるのを防ぐため変数
    private int turnSequenceID = 0;     // ターンの切り替えにおいて、古いコルーチンを無効化するための番号


    // Awake()には、他シーンからバトルシーンに移動した時に、手持ちデータとしてセットした3体モンスターデータから
    // 30枚のカードデッキ情報を読み出して、deckManagerを用いて山札にセットする処理を行う予定
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

        enemyAreaManager.SetEnemyData(firstPhase.GetEnemiesList());

        allyAreaManager.SetAllyData(battleSessionData.GetPlayerAlliesList().ConvertAll(ally => (MonsterData)ally));

        // デッキを作成
        deckManager.ClearDeck();
        foreach (var cardData in battleSessionData.GetPlayerCardsList())
        {
            deckManager.AddCardToDeck(cardData);
        }

        // デッキが作成されたら、山札をシャッフルする
        deckManager.ShuffleDeck();
    }

    void Start()
    {

        // 敵を生成
        enemyAreaManager.SpawnEnemies();
        allyAreaManager.SpawnAllies();

        // ゲーム開始時の手札5枚ドロー。ゲーム開始時の手札は5枚。
        deckManager.DrawInitialHand();

        // Fireball(火の玉)がクリックされて、その効果を決定する際に
        // 処理されるイベントメソッドを登録
        fireballManager.OnFireballEffectTriggered += HandleFireballEffect;

        UpdatePhaseUI();

        StartCoroutine(BattleStartRoutine());  // ゲーム開始の通知やログ表示のためのコルーチンを始動

    }

    private IEnumerator BattleStartRoutine()
    {
        battleState = BattleState.TurnTransition;

        yield return battleNoticeManager.Show(BattleNoticeType.BattleStart);  // 「バトル開始」の通知を表示
        Log("バトル開始！", BattleLogType.System);  // 「バトル開始」の戦闘ログ表示

        // 1ターン目開始
        StartPlayerTurn();
    }

    // プレイヤーターン開始
    private void StartPlayerTurn()
    {
        turnSequenceID++;
        StartCoroutine(PlayerTurnRoutine(turnSequenceID));
    }

    private IEnumerator PlayerTurnRoutine(int sequenceID)
    {
        
        battleState = BattleState.TurnTransition;  // バトルの状態を「演出中」にする

        yield return battleNoticeManager.Show(BattleNoticeType.PlayerTurn);

        Log("プレイヤーのターン開始！", BattleLogType.Attention);


        allyAreaManager.ProcessHealOverTime();  // 継続回復の処理を行う
        yield return new WaitForSeconds(1.0f);

        battleState = BattleState.PlayerTurn; // その後、プレイヤーターン状態にする

        // マナ回復（ターン数に応じて使用可能マナ増加）
        manaManager.StartTurn();
        DrawCardAtTurnStart(); // ターン開始時に手札が5枚になるまでカードをドローする(但し、手札が4枚以下の場合のみ)
        isFirstTurn = false;  // 1ターン目が終了すると、それ以降はフラグがfalseに。

        RefreshHandUI();  // 手札データを入手して、それを元に手札UIを表示。

        handAreaManager.SetVisible(true); // 手札の表示
        handAreaManager.SetInteractable(true);  // 手札を明るく表示し、操作可能状態に
        endTurnButton.SetActive(true);  // プレイヤーターン終了ボタンを表示
        fireballManager.StartSpawning();  // fireballの生成を開始する
    }

    /// プレイヤーがカードを出したときに、そのカードの効果を使用する処理。マナが足りない場合は、効果を適用しない。
    /// dropPositionは、カードをドロップした位置。
    /// System.Action は C# 標準のデリゲート型。Action自体は引数なしのメソッド型。<T>のは引数Tがという意味。
    /// isSuccessCallbackは、カードを使用できたかどうかをboolで通知し、それに応じた処理を行うメソッドを登録するデリゲート。
    /// 成功なら true、失敗なら false。
    /// CardUI_DragDrop側でカード削除や元の位置に戻す処理が記載されている(第二引数isSuccessCallbackの処理内容)
    public void PlayCard(Card card, Vector2 dropPosition, System.Action<bool> isSuccessCallback)
    {
        if (battleState != BattleState.PlayerTurn) // プレイヤーターン中でない場合、
        {
            // カードの効果を適用せずにそのままリターン
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

        bool playSuccess = isSuccessResolveCardAndEffect(card, dropPosition);  // カードの種類に応じて効果を適用。

        if (playSuccess)
        {
            deckManager.DiscardCard(card);  // 使用したカードは墓地へ
            RefreshHandUI(); // カード使用後に、残った手札カード情報で手札UIを更新する
        }
        isSuccessCallback?.Invoke(playSuccess);  // カードを使用できたことを(isSuccessCallbackに登録しているメソッドに)通知して、その時の処理を行う。

        // 敵が全滅して、バトルが終了しているかをチェック
        StartCoroutine(CheckBattleEnd()); 
    }

    // カードの種類に応じて、味方や敵に効果を適用するメソッド。カードの効果発動が解決すれば、trueを返す
    private bool isSuccessResolveCardAndEffect(Card card, Vector2 dropPosition)
    {
        bool playSuccess = false;
        int targetIndex = -1;
        switch (card.GetCardEffectType())
        {
            case CardEffectType.AttackToSelected:  // 選択している敵への単体攻撃
                targetIndex = enemyAreaManager.GetSelectedEnemyIndex(dropPosition);
                enemyAreaManager.TakeDamageToTargetEnemy(targetIndex, card.GetPower());
                playSuccess = true;
                AudioManager.Instance.PlaySE(AttackSoundEffect);
                break;

            case CardEffectType.AttackToAll:      // 敵全体への攻撃
                enemyAreaManager.TakeDamageToAll(card.GetPower());
                AudioManager.Instance.PlaySE(AttackSoundEffect);
                playSuccess = true;
                break;

            case CardEffectType.Heal:             // 味方HPを回復
                allyAreaManager.HealSharedHP(card.GetPower());
                AudioManager.Instance.PlaySE(HealSoundEffect);
                playSuccess = true;
                break;

            // case CardEffectType.Buff:             // 味方にバフを与えて強化
            //     allyAreaManager.ApplyBuff(card.GetPower(), card.GetDurationTurn());
            //     break;

            case CardEffectType.DamageOverTime:   // 敵単体への継続ダメージ
                targetIndex = enemyAreaManager.GetSelectedEnemyIndex(dropPosition);
                enemyAreaManager.ApplyDamageOverTimeToTargetEnemy(targetIndex, card.GetPower(), card.GetDurationTurn());
                playSuccess = true;
                break;

            case CardEffectType.HealOverTime:     // 味方HPの継続回復
                allyAreaManager.ApplyHealOverTime(card.GetPower(), card.GetDurationTurn());
                playSuccess = true;
                break;
        }
        return playSuccess;
    }

    // プレイヤーがターンエンド
    public void EndPlayerTurn()
    {
        if (battleState != BattleState.PlayerTurn) return;  // バトル状態がプレイヤーターンでなければ、何もしない。

        endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
        handAreaManager.SetInteractable(false); // 手札を暗く表示し、操作不可能状態に
        fireballManager.StopSpawning();  // fireballの生成を停止する
        EnemyTurn();
    }

    //　敵ターン
    private void EnemyTurn()
    {
        turnSequenceID++;
        StartCoroutine(EnemyTurnRoutine(turnSequenceID));
    }

    private IEnumerator EnemyTurnRoutine(int sequenceID)
    {
        battleState = BattleState.TurnTransition;  // バトルの状態を「演出中」にする

        yield return battleNoticeManager.Show(BattleNoticeType.EnemyTurn);
        Log("敵のターン!", BattleLogType.Attention);

        // 継続ダメージ効果を適用し、敵が全滅したかを判定
        enemyAreaManager.ProcessDamageOverTime();
        yield return new WaitForSeconds(1.0f);
        // 継続ダメージにより、敵が全滅し、バトルが終了したかをチェック
        yield return StartCoroutine(CheckBattleEnd());
        if (sequenceID != turnSequenceID) yield break;

        yield return new WaitForSeconds(0.8f);  // 演出の待ち時間


        battleState = BattleState.EnemyTurn;  // その後、敵ターン状態にする

        // 各敵の攻撃力(attackPower)を元に、味方の共有HPに与えるダメージ量を[power-10, power+30]の範囲でランダムに決定
        enemyAreaManager.PrepareEnemyAttackAmounts();

        // 決定したダメージ量(生存している敵の数分)を味方共有HPに与える
        // この処理が終わるまで待つ
        yield return StartCoroutine(AttackToAllySharedHP());

        // プレイヤーが死亡し、バトルが終了したか(ゲームオーバーか)を確かめ、ゲーム終了していたら
        // コルーチンも終了させる
        yield return StartCoroutine(CheckBattleEnd());
        if (battleState == BattleState.GameOver) yield break;
        if (sequenceID != turnSequenceID) yield break;

        // 次のプレイヤーターンへ
        StartPlayerTurn();
    }

    // 味方共有HPにダメージを与えるメソッド
    private IEnumerator AttackToAllySharedHP()
    {
        IReadOnlyList<int> enemyPowersList = enemyAreaManager.GetEnemyPowersList();  // 各敵の攻撃量を保持するリストを参照
        IReadOnlyList<string> aliveEnemyNamesList = enemyAreaManager.GetAliveEnemyNamesList();  // 生存している敵の名前を保持するリストを参照

        for (int i = 0; i < enemyPowersList.Count; i++)
        {
            if (i >= aliveEnemyNamesList.Count) break;
            string attackerName = aliveEnemyNamesList[i];
            int power = enemyPowersList[i];
            Log($"{attackerName}の攻撃！", BattleLogType.Attack);
            yield return new WaitForSeconds(1.0f);  // ログ表示後の待ち時間

            allyAreaManager.TakeDamageToSharedHP(power); // 味方側の共有HPにpower分のダメージ
            AudioManager.Instance.PlaySE(AttackSoundEffect); // 攻撃時の効果音を出す

            yield return new WaitForSeconds(1.0f);      // 攻撃後は少し間を空ける
        }
    }

    // 手札カードデータを入手して、手札UIを更新するメソッド
    private void RefreshHandUI()
    {
        // deckManagerによってセットされた手札データを入手(ゲット)して、
        // そのデータをhandAreaManagerにセット
        handAreaManager.setHandCardData(deckManager.GetHand());
        // セットした手札データを元に手札カードオブジェクトを生成し、UI表示。
        // そのとき、プレイヤーがカードを使用した時に発生するイベントPlayCard()メソッド(メソッドへのポインタ)を渡す。
        handAreaManager.UpdateHandUI((card, dropPos, callback) => PlayCard(card, dropPos, callback), enemyAreaManager, allyAreaManager);
    }

    // ターン開始時にカードを1枚ドローするメソッド。但し、1ターン目の場合や手札が5枚以上ある場合はドローしない(手札は常に5枚以下)
    private void DrawCardAtTurnStart()
    {
        if (!isFirstTurn && deckManager.GetHandCount() < 5)  // 2ターン目以降で、手札カードが4枚以下なら
        {
            deckManager.DrawCardFull();  // カードを1枚ドローする(1ターン目は手札は5枚で、以降のターンは開始時に手札が5枚になるまでドロー)
        }
    }

    // バトルが終了しているかを確認し、その結果に応じた処理をし、
    // 終了状態をtrueとして返すメソッド
    private IEnumerator CheckBattleEnd()
    {
        if (isEndingBattle) yield break;

        // 敵が全滅していたら、「勝利」として処理する
        if (enemyAreaManager.GetIsAllMonstersDead())
        {
            isEndingBattle = true;
            Log("敵は全滅した！", BattleLogType.Attention);
            handAreaManager.SetVisible(false);  // 手札の非表示
            endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
            fireballManager.StopSpawning();  // fireballの生成を停止する

            // 現在のステージ情報(勝利したステージの情報)を取得し、
            // そのステージの現在のフェーズのインデックスを取得。
            // 次のフェーズがあれば、その処理を、
            // 次のフェーズが無ければ、現在のステージを
            // 「クリア済みのステージ」としてPlayerDataにステージIDを登録する
            var battleSessionData = BattleSessionData.Instance;
            var currentStage = battleSessionData.GetCurrentStage();

            if (currentStage != null)
            {
                int currentPhaseIndex = battleSessionData.GetCurrentPhaseIndex();
                StagePhase currentPhase = currentStage.GetStagePhases()[currentPhaseIndex];

                int clearRewardPoints = currentPhase.GetClearRewardPoints();

                yield return battleNoticeManager.ShowVictoryAndReward(clearRewardPoints);  // 勝利した際の通知・報酬の表示

                GameManager.Instance.PlayerData.AddPoints(clearRewardPoints);

                if (battleSessionData.IsThereNextPhase())
                {
                    yield return StartCoroutine(StartNextPhaseRoutine());
                }
                else
                {
                    // 最終フェーズのみステージクリア
                    Log("ステージクリア！", BattleLogType.Attention);

                    yield return battleNoticeManager.Show(BattleNoticeType.StageClear);

                    GameManager.Instance.PlayerData.SetClearedStage(currentStage.GetStageID());

                    yield return StartCoroutine(ReturnToLabScene()); // LabSceneに戻る
                }
            }
        }

        // 味方(プレイヤー)が死んでいたら、「GameOver」として処理する
        if (allyAreaManager.GetIsDead())
        {
            isEndingBattle = true;
            battleState = BattleState.GameOver;
            Log("味方は全滅した…", BattleLogType.Attention);
            yield return StartCoroutine(battleNoticeManager.Show(BattleNoticeType.GameOver));    // 敗北した際の通知の表示
            handAreaManager.SetVisible(false);  // 手札の非表示
            endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
            fireballManager.StopSpawning();  // fireballの生成を停止する
            StartCoroutine(ReturnToLabScene());  // LabSceneに戻る
        }
    }

    // 次のフェーズへ行く際の処理を行うコルーチン
    private IEnumerator StartNextPhaseRoutine()
    {
        battleState = BattleState.TurnTransition;
        Log("次のフェーズへ！", BattleLogType.System);
        yield return StartCoroutine(battleNoticeManager.Show(BattleNoticeType.NextPhase));

        var battleSessionData = BattleSessionData.Instance;
        battleSessionData.AdvancePhase();

        var currentStage = battleSessionData.GetCurrentStage();

        StagePhase nextPhase = currentStage.GetStagePhases()[battleSessionData.GetCurrentPhaseIndex()];

        enemyAreaManager.ClearAllDamageOverTime();  // 前フェーズで残っているDOTを削除

        enemyAreaManager.SetEnemyData(nextPhase.GetEnemiesList());

        enemyAreaManager.SpawnEnemies();

        manaManager.ResetMana();

        UpdatePhaseUI();

        isEndingBattle = false;

        yield return new WaitForSeconds(1.5f);
        StartPlayerTurn();
    }

    // 数秒待ってからラボシーンへ戻るコルーチン
    private IEnumerator ReturnToLabScene()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.GoToLab();  // ラボシーンへ移動
    }



    // 戦闘ログにメッセージを追加するメソッド。メッセージのタイプも引数として与えること。
    private void Log(string message, BattleLogType type)
    {
        // シングルトンインスタンスであるBattleLogManagerインスタンスに追加したいログを送る
        BattleLogManager.Instance.AddLog(message, type);
        Debug.Log(message);  // デバッグログとしても表示する
    }

    private void UpdatePhaseUI()
    {
        var battleSessionData = BattleSessionData.Instance;
        var currentStage = battleSessionData.GetCurrentStage();
        stagePhaseUI.text =
            battleSessionData.GetCurrentPhaseIndex() + 1 + " / " + currentStage.GetStagePhases().Count;
    }


    // Fireball(火の玉)の効果の種類に対応した処理
    // Fireballがクリックされた際に発動するイベントとして登録されている
    private void HandleFireballEffect(FireballEffectResult effectResult)
    {
        if (battleState != BattleState.PlayerTurn) // プレイヤーターン以外なら、Fireball効果は無効
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

        // 　バトルが終了(勝利 or GameOver)しているかを確かめる。
        StartCoroutine(CheckBattleEnd());
    }
}

// バトル中の状態を5タイプに分別
// enumは列挙型
public enum BattleState
{
    PlayerTurn,  // プレイヤーの手番
    EnemyTurn,   // 敵の手番
    TurnTransition,  // 演出中の状態
    Victory,        // 勝利状態
    GameOver        // ゲームオーバーの状態
}
