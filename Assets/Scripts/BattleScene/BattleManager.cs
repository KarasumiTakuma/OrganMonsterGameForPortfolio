using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private HandAreaManager handAreaManager; // HandAreaManagerの参照
    [SerializeField] private EnemyAreaManager enemyAreaManager;
    [SerializeField] private AllyAreaManager allyAreaManager;
    [SerializeField] private List<CardData> cardDataList;
    [SerializeField] private GameObject endTurnButton;  // プレイヤーターンの終了用ボタン
    [SerializeField] private AudioClip AttackSoundEffect;  // 攻撃の効果音
    [SerializeField] private AudioClip HealSoundEffect; // 回復の効果音


    [Header("UI")]
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text enemyHPText;

    private bool playerTurn = true;  // プレイヤーターンであるかのフラグ。
    private bool isFirstTurn = true; // 最初のターンであるかどうかのフラグ。


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

        Log("バトル開始！");

        // 1ターン目開始
        StartPlayerTurn();
    }

    /// <summary>
    /// プレイヤーターン開始
    /// </summary>
    private void StartPlayerTurn()
    {
        playerTurn = true;

        // マナ回復（ターン数に応じて使用可能マナ増加）
        manaManager.StartTurn();
        DrawCardAtTurnStart(); // ターン開始時に手札が5枚になるまでカードをドローする(但し、手札が4枚以下の場合のみ)
        isFirstTurn = false;  // 1ターン目が終了すると、それ以降はフラグがfalseに。

        RefreshHandUI();  // 手札データを入手して、それを元に手札UIを表示。

        endTurnButton.SetActive(true);  // プレイヤーターン終了ボタンを表示

        Log("プレイヤーのターン開始！");
    }

    /// プレイヤーがカードを出したときに、そのカードの効果を使用する処理。マナが足りない場合は、効果を適用しない。
    /// System.Action は C# 標準のデリゲート型。Action自体は引数なしのメソッド型。<T>のは引数Tがという意味。
    /// isSuccessCallbackは、カードを使用できたかどうかをboolで通知し、それに応じた処理を行うメソッドを登録するデリゲート。
    /// 成功なら true、失敗なら false。
    /// CardUI_DragDrop側でカード削除や元の位置に戻す処理が記載されている(第二引数isSuccessCallbackの処理内容)
    public void PlayCard(Card card, System.Action<bool> isSuccessCallback)
    {
        if (!playerTurn) // プレイヤーターンでない場合、
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

            // 敵全滅チェック
            if (enemyAreaManager.GetIsAliveMonsterCount() == 0)
            {
                Log("敵は全滅した！");
                return;
            }

        }
        else
        {
            Log("マナが足りません！");
            isSuccessCallback?.Invoke(false); // カードを使用できなかったことを通知して、その時の処理を行う。
        }
    }

    /// <summary>
    /// プレイヤーがターンエンド
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!playerTurn) return;

        playerTurn = false;
        endTurnButton.SetActive(false);  // プレイヤーターン終了ボタンを非表示

        Log("プレイヤーのターン終了！");
        EnemyTurn();
    }

    /// <summary>
    /// 敵ターン
    /// </summary>
    private void EnemyTurn()
    {
        Log("敵のターン!");

        // 各敵の攻撃力(attackPower)を元に、味方の共有HPに与えるダメージ量を[power-3, power+3]の範囲でランダムに決定
        enemyAreaManager.EnemyRundomPowers();

        AttackToAllySharedHP();  // 決定したダメージ量(敵3体分)を味方共有HPに与える
        UpdateHPUI();

        // プレイヤーが倒れたら敗北
        if (!allyAreaManager.GetIsAliveMonster())
        {
            Log("味方は全滅した…");
            return;
        }

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

    // 各敵のスポーン位置(EnemyArea/SpawnPoint{1,2,3})をクリックした際に呼ばれるメソッド。
    // プレイヤーが敵をクリックしたときに呼ばれるメソッド。
    public void ClickedEnemy(int index)
    {
        // 現在選択されている敵のインデックスを取得
        int currentSelectedIndex = enemyAreaManager.GetSelectedEnemyIndex();

        if (currentSelectedIndex == index)  // 現在選択されている敵をプレイヤーがクリックしたなら
        {
            //　選択を解除する処理を行う
            //　EnemyAreaManager.NoSelectionは、選択していない状態を表す定数
            enemyAreaManager.UpdateSelectedEnemy(EnemyAreaManager.NoSelection);
        }
        else
        {
            // 別の敵をクリックした場合、その敵を新しく選択する
            enemyAreaManager.UpdateSelectedEnemy(index);
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

    private void AttackToAllySharedHP()
    {
        List<int> enemyPowersList = enemyAreaManager.GetEnemyPowersList();
        foreach (int power in enemyPowersList)
        {
            allyAreaManager.TakeDamageToSharedHP(power); // 味方側の共有HPにpower分のダメージ
        }
        AudioManager.Instance.PlaySE(AttackSoundEffect);
    }

    // 戦闘ログにメッセージを追加するメソッド
    private void Log(string message)
    {
        // シングルトンインスタンスであるBattleLogManagerインスタンスに追加したいログを送る
        BattleLogManager.Instance.AddLog(message);  
        Debug.Log(message);  // デバッグログとしても表示する
    }
}
