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
    [SerializeField] private Sprite defaultCardSprite; // カード画像表示用(一時的)


    [Header("UI")]
    [SerializeField] private TMP_Text logText; // 戦闘ログ
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text enemyHPText;

    private bool playerTurn = true;

    // Awake()は、今のところカードUI表示テストのために設定してある(おそらく後に消す)
    void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            CardData cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.cardName = "攻撃カード";
            cardData.manaCost = 0;
            cardData.cardType = CardType.AttackToSelected;
            cardData.power = 20;
            cardData.cardImage = defaultCardSprite;

            deckManager.AddCardToDeck(new Card(cardData)); // 本来なら、deckManager.drowInitialHand()
        }

        deckManager.ShuffleDeck();
    }

    void Start()
    {
        // 敵を生成
        enemyAreaManager.SpawnEnemies();
        allyAreaManager.SpawnAllies();

        // ゲーム開始時の手札5枚ドロー
        deckManager.DrawInitialHand();
        List<Card> handCardsData = deckManager.GetHand(); // deckManagerによってセットされた手札5枚を入手
        // 5枚の手札データをhandAreaManagerにセット(後にこのデータを付与した手札カードオブジェクトを生成)
        handAreaManager.setHandCardData(handCardsData);

        // handAreaManagerにドローした手札5枚のオブジェクトを生成して表示するよう指示する。
        // UpdateHandUIで手札オブジェクトを生成する際に、プレイヤーがカードを使用した時に
        // 発生するイベントPlayCard()メソッド(メソッドへのポインタ)を渡す。
        handAreaManager.UpdateHandUI(PlayCard);

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

        // ターン開始時に1枚だけドロー
        deckManager.DrawCard();

        // 手札UIを更新
        handAreaManager.UpdateHandUI(PlayCard);

        Log("プレイヤーのターン開始！");
    }

    /// <summary>
    /// プレイヤーがカードを使用
    /// </summary>
    public void PlayCard(Card card)
    {
        if (!playerTurn) return;

        // マナが足りるか確認
        if (manaManager.UseMana(card.GetManaCost()))
        {
            // カード効果を適用
            switch (card.GetCardType())
            {
                case CardType.AttackToSelected:
                    enemyAreaManager.TakeDamageToSelectedEnemy(card.GetPower());
                    Log($"敵単体に{card.GetPower()}ダメージ！");
                    break;
                // case  CardType.AttackToAll:     // CardType.AttackToAllをCradDataに追加予定。全体攻撃タイプのカードを使用した際に。
                //     enemyAreaManager.TakeDamageToAll(card.GetPower());
                //     Log($"敵全体に{card.GetPower()}ダメージ！");
                //     break;
                case CardType.Heal:
                    allyAreaManager.HealSharedHP(card.GetPower()); // 味方全体回復
                    Log($"味方全体が{card.GetPower()}回復！");
                    break;
            }

            UpdateHPUI();
            deckManager.DiscardCard(card);  // 使用したカードは墓地へ
            handAreaManager.UpdateHandUI(PlayCard); // 手札UIを更新


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
        }
    }

    /// <summary>
    /// プレイヤーがターンエンド
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!playerTurn) return;

        playerTurn = false;
        Log("プレイヤーのターン終了！");
        EnemyTurn();
    }

    /// <summary>
    /// 敵ターン
    /// </summary>
    private void EnemyTurn()
    {
        // シンプルに敵が固定ダメージ
        int damage = 5;
        allyAreaManager.TakeDamage(damage); // 味方側の共有HPにダメージ
        Log($"敵の攻撃！ プレイヤーに{damage}ダメージ！");
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
    public void SelectEnemy(int index)
    {
        enemyAreaManager.SetSelectedEnemy(index);
    }

    /// <summary>
    /// 戦闘ログにメッセージを追加
    /// </summary>
    private void Log(string message)
    {
        if (logText != null)
        {
            logText.text += message + "\n";
        }
        Debug.Log(message);
    }
}
