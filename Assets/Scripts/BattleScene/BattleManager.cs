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
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text enemyHPText;

    private bool isFirstTurn = true; // 最初のターンであるかどうかのフラグ。

    private BattleState battleState; // バトル中の状態をBattleState型として保持するための変数


    // Awake()には、他シーンからバトルシーンに移動した時に、手持ちデータとしてセットした3体モンスターデータから
    // 30枚のカードデッキ情報を読み出して、deckManagerを用いて山札にセットする処理を行う予定
    void Awake()
    {

        var session = BattleSessionData.Instance;
        if (session == null)
        {
            Debug.LogError("BattleSessionData が見つかりません！");
            return;
        }

        enemyAreaManager.SetEnemyData(session.GetCurrentStage().GetEnemiesList());

        allyAreaManager.SetAllyData(session.GetPlayerAlliesList().ConvertAll(ally => (MonsterData)ally));

        // デッキを作成
        deckManager.ClearDeck();
        foreach (var cardData in session.GetPlayerCardsList())
        {
            deckManager.AddCardToDeck(cardData);
        }

        // デッキが作成されたら、山札をシャッフルする
        deckManager.ShuffleDeck();

        // データクリア（次回ステージ選択に備える）
        session.ClearData();
    }

    void Start()
    {

        // 敵を生成
        enemyAreaManager.SpawnEnemies();
        allyAreaManager.SpawnAllies();

        // ゲーム開始時の手札5枚ドロー。ゲーム開始時の手札は5枚。
        deckManager.DrawInitialHand();

        UpdateHPUI();

        // Fireball(火の玉)がクリックされて、その効果を決定する際に
        // 処理されるイベントメソッドを登録
        fireballManager.OnFireballEffectTriggered += HandleFireballEffect;

        StartCoroutine(BattleStartRoutine());  // ゲーム開始の通知やログ表示のためのコルーチンを始動
        
    }

    private IEnumerator BattleStartRoutine()
    {
        battleState = BattleState.TurnTransition;

        battleNoticeManager.Show(BattleNoticeType.BattleStart);  // 「バトル開始」の通知を表示
        Log("バトル開始！", BattleLogType.System);  // 「バトル開始」の戦闘ログ表示

        yield return new WaitForSeconds(2.0f);  // バトル開始の通知、ログ表示時間

        // 1ターン目開始
        StartPlayerTurn();
    }

    // プレイヤーターン開始
    private void StartPlayerTurn()
    {
        StartCoroutine(PlayerTurnRoutine());
    }

    private IEnumerator PlayerTurnRoutine()
    {
        // playerTurn = true;

        battleState = BattleState.TurnTransition;  // バトルの状態を「演出中」にする


        battleNoticeManager.Show(BattleNoticeType.PlayerTurn);

        Log("プレイヤーのターン開始！", BattleLogType.Attention);
        
        yield return new WaitForSeconds(1.5f);  // 少し間を空ける

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
    /// System.Action は C# 標準のデリゲート型。Action自体は引数なしのメソッド型。<T>のは引数Tがという意味。
    /// isSuccessCallbackは、カードを使用できたかどうかをboolで通知し、それに応じた処理を行うメソッドを登録するデリゲート。
    /// 成功なら true、失敗なら false。
    /// CardUI_DragDrop側でカード削除や元の位置に戻す処理が記載されている(第二引数isSuccessCallbackの処理内容)
    public void PlayCard(Card card, System.Action<bool> isSuccessCallback)
    {
        if (battleState != BattleState.PlayerTurn) // プレイヤーターン中でない場合、
        {
            // カードの効果を適用せずにそのままリターン
            isSuccessCallback?.Invoke(false);
            return;
        }

        // マナが足りるか確認
        if (manaManager.UseMana(card.GetManaCost()))
        {
            // カード効果を適用
            switch (card.GetCardType())
            {
                case CardType.AttackToSelected:
                    enemyAreaManager.TakeDamageToSelectedEnemy(card.GetPower());
                    AudioManager.Instance.PlaySE(AttackSoundEffect);
                    break;
                case CardType.AttackToAll:     // CardType.AttackToAllをCradDataに追加予定。全体攻撃タイプのカードを使用した際に。
                    enemyAreaManager.TakeDamageToAll(card.GetPower());
                    AudioManager.Instance.PlaySE(AttackSoundEffect);
                    break;
                case CardType.Heal:
                    allyAreaManager.HealSharedHP(card.GetPower()); // 味方全体回復
                    AudioManager.Instance.PlaySE(HealSoundEffect);
                    break;
            }

            UpdateHPUI();
            deckManager.DiscardCard(card);  // 使用したカードは墓地へ
            RefreshHandUI(); // カード使用後に、残った手札カード情報で手札UIを更新する
            isSuccessCallback?.Invoke(true);  // カードを使用できたことを(isSuccessCallbackに登録しているメソッドに)通知して、その時の処理を行う。

            CheckBattleEnd();    // 敵が全滅して、ゲームが終了しているかをチェック
        }
        else
        {
            Log("マナが足りません！", BattleLogType.Attention);
            isSuccessCallback?.Invoke(false); // カードを使用できなかったことを通知して、その時の処理を行う。
        }
    }

    // プレイヤーがターンエンド
    public void EndPlayerTurn()
    {
        if (battleState != BattleState.PlayerTurn) return;  // バトル状態がプレイヤーターンでなければ、何もしない。

        endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
        handAreaManager.SetInteractable(false); // 手札を暗く表示し、操作不可能状態に
        fireballManager.StopSpawning();  // fireballの生成を停止する
        Log("プレイヤーのターン終了！", BattleLogType.Attention);
        EnemyTurn();
    }

    //　敵ターン
    private void EnemyTurn()
    {
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        battleState = BattleState.TurnTransition;  // バトルの状態を「演出中」にする

        battleNoticeManager.Show(BattleNoticeType.EnemyTurn);
        Log("敵のターン!", BattleLogType.Attention);

        yield return new WaitForSeconds(3.0f);  // ターン開始演出の待ち時間

        battleState = BattleState.EnemyTurn;  // その後、敵ターン状態にする

        // 各敵の攻撃力(attackPower)を元に、味方の共有HPに与えるダメージ量を[power-3, power+3]の範囲でランダムに決定
        enemyAreaManager.PrepareEnemyAttackAmounts();

        // 決定したダメージ量(敵3体分)を味方共有HPに与える
        // この処理が終わるまで待つ
        yield return StartCoroutine(AttackToAllySharedHP());

        // プレイヤーが死亡し、ゲームが終了したかを確かめ、ゲーム終了していたら
        // コルーチンも終了させる
        if(CheckBattleEnd()) yield break;

        // 次のプレイヤーターンへ
        StartPlayerTurn();
    }

    /// <summary>
    /// HP表示更新
    /// </summary>
    private void UpdateHPUI()
    {
        if (playerHPText)
            playerHPText.text = $"HP: {allyAreaManager.GetSharedCurrentHP()}";
        if (enemyHPText)
            enemyHPText.text = $"HP: ";
    }

    // 各敵のスポーン位置(EnemyArea/SpawnPoint{1,2,3})に応じた敵キャラのImageをクリックした際に呼ばれるメソッド。
    // プレイヤーが敵をクリックしたときに呼ばれるメソッド。
    // ClickedEnemyのtargetIndexの対象は、各敵のスポーン位置(EnemyArea/SpawnPoint{1,2,3})
    // に対応した番号(左から {0,1,2})であるが、実際にクリック判定の対象となるのは各スポーン位置に応じた
    // 敵のImageである(スポーン位置に生成するEnemyPrefabのEnemyCharacterがImageコンポーネントを持ち、
    // そのRaycastTargetがONになっているから。)。なので、敵が死んだ場合はその敵がスポーンしていた場所(pawnPoint)
    // をプレイヤークリックしても、反応しない
    public void ClickedEnemy(int targetIndex)
    {
        // 現在選択されている敵のインデックスを取得
        int currentSelectedIndex = enemyAreaManager.GetSelectedEnemyIndex();

        if (currentSelectedIndex == targetIndex)  // 現在選択されている敵をプレイヤーがクリックしたなら
        {
            //　選択を解除する処理を行う
            //　EnemyAreaManager.NoSelectionは、選択していない状態を表す定数
            enemyAreaManager.UpdateSelectedEnemy(EnemyAreaManager.NoSelection);
        }
        else
        {
            // 別の敵をクリックした場合、その敵を新しく選択する
            enemyAreaManager.UpdateSelectedEnemy(targetIndex);
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
        handAreaManager.UpdateHandUI(PlayCard);
    }

    // ターン開始時にカードを1枚ドローするメソッド。但し、1ターン目の場合や手札が5枚以上ある場合はドローしない(手札は常に5枚以下)
    private void DrawCardAtTurnStart()
    {
        if (!isFirstTurn && deckManager.GetHandCount() < 5)  // 2ターン目以降で、手札カードが4枚以下なら
        {
            deckManager.DrawCardFull();  // カードを1枚ドローする(1ターン目は手札は5枚で、以降のターンは開始時に手札が5枚になるまでドロー)
        }
    }

    // 味方共有HPにダメージを与えるメソッド
    private IEnumerator AttackToAllySharedHP()
    {
        List<int> enemyPowersList = enemyAreaManager.GetEnemyPowersList();  // 各敵の攻撃量を保持するリストを参照
        foreach (int power in enemyPowersList) //　各敵の攻撃量powerごとに
        {
            allyAreaManager.TakeDamageToSharedHP(power); // 味方側の共有HPにpower分のダメージ
            AudioManager.Instance.PlaySE(AttackSoundEffect);  // 攻撃時の効果音を出す

            UpdateHPUI();  // HPテキストの更新

            yield return new WaitForSeconds(0.8f);  // 攻撃後は少し間を空ける
        }
    }

    // ゲームが終了しているかを確認し、その結果に応じた処理をし、
    // 終了状態をtrueとして返すメソッド
    private bool CheckBattleEnd()
    {

        // 敵が全滅していたら、「勝利」として処理する
        if (enemyAreaManager.GetIsAllMonstersDead())
        {
            battleState = BattleState.Victory;
            Log("敵は全滅した！", BattleLogType.Attention);
            Log("プレイヤーの勝利！", BattleLogType.Attention);
            battleNoticeManager.Show(BattleNoticeType.Victory);  // 勝利した際の通知の表示
            handAreaManager.SetVisible(false);  // 手札の非表示
            endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
            fireballManager.StopSpawning();  // fireballの生成を停止する

            // 現在のステージ情報を取得し、「クリア済みのステージ」としてPlayerDataにステージIDを登録する
            var currentStage = BattleSessionData.Instance.GetCurrentStage();
            if(currentStage != null)
            {
                GameManager.Instance.PlayerData.SetClearedStage(currentStage.GetStageID());
            }

            return true;
        }

        // 味方(プレイヤー)が死んでいたら、「GameOver」として処理する
        if (allyAreaManager.GetIsDead())
        {
            battleState = BattleState.GameOver;
            Log("味方は全滅した…", BattleLogType.Attention);
            battleNoticeManager.Show(BattleNoticeType.GameOver);    // 敗北した際の通知の表示
            handAreaManager.SetVisible(false);  // 手札の非表示
            endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示
            fireballManager.StopSpawning();  // fireballの生成を停止する
            return true;
        }

        return false;
        
    }

    // 戦闘ログにメッセージを追加するメソッド。メッセージのタイプも引数として与えること。
    private void Log(string message, BattleLogType type)
    {
        // シングルトンインスタンスであるBattleLogManagerインスタンスに追加したいログを送る
        BattleLogManager.Instance.AddLog(message, type);  
        Debug.Log(message);  // デバッグログとしても表示する
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

        UpdateHPUI();  // HPテキストの更新

        CheckBattleEnd();  // ゲームが終了(勝利 or GameOver)しているかを確かめる。
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
