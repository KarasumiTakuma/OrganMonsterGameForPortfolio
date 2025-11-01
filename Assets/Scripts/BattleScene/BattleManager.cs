using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ManaManager manaManager;
    [SerializeField] private HandAreaManager handAreaManager; // HandAreaManagerの参照
    [SerializeField] private EnemyAreaManager enemyAreaManager;
    [SerializeField] private Sprite defaultCardSprite; // カード画像表示用(一時的)


    [Header("Player & Enemy HP")]
    [SerializeField] private int playerHP = 30;
    [SerializeField] private int enemyHP = 30;
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text enemyHPText;



    [Header("UI")]
    [SerializeField] private TMP_Text logText; // 戦闘ログ

    private bool playerTurn = true;

    // Awake()は、今のところカードUI表示テストのために設定してある(おそらく後に消す)
    void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            CardData cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.cardName = "攻撃カード";
            cardData.manaCost = 2;
            cardData.cardType = CardType.Attack;
            cardData.power = 5;
            cardData.cardImage = defaultCardSprite;

            deckManager.AddCardToDeck(new Card(cardData)); // 本来なら、deckManager.drowInitialHand()
        }

        deckManager.ShuffleDeck();
    }

    void Start()
    {
        // 敵を生成
        enemyAreaManager.SpawnEnemies();

        // ゲーム開始時の手札5枚ドロー
        deckManager.DrawInitialHand();

        handAreaManager.UpdateHandUI();

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
        handAreaManager.UpdateHandUI();

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
                case CardType.Attack:
                    enemyHP -= card.GetPower();
                    Log($"敵に{card.GetPower()}ダメージ！");
                    break;
                case CardType.Heal:
                    playerHP += card.GetPower();
                    Log($"プレイヤーが{card.GetPower()}回復！");
                    break;
                    // 他のカードタイプも追加可能
                    //
                    //
            }

            UpdateHPUI();
            deckManager.DiscardCard(card);  // 使用したカードは墓地へ
            handAreaManager.UpdateHandUI(); // 手札UIを更新

            // 敵が倒れたら勝利
            if (enemyHP <= 0)
            {
                Log("敵を倒した！");
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
        playerHP -= damage;
        Log($"敵の攻撃！ プレイヤーに{damage}ダメージ！");
        UpdateHPUI();

        // プレイヤーが倒れたら敗北
        if (playerHP <= 0)
        {
            Log("プレイヤーは倒れた…");
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
        if (playerHPText) playerHPText.text = $"HP: {playerHP}";
        if (enemyHPText) enemyHPText.text = $"HP: {enemyHP}";
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
