using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// バトルシーンに登場するモンスターの共通基底クラス。
/// HP・攻撃力などの基本ステータス管理、
/// ダメージ演出やハイライト表示といった共通挙動を提供する。
/// Enemy / Ally クラスは本クラスを継承して実装される。
/// </summary>
public class Monster : MonoBehaviour
{
    /// <summary>モンスターを一意に識別するID（MonsterData由来）</summary>
    protected int monsterID;

    /// <summary>モンスターの表示名</summary>
    protected string monsterName;

    /// <summary>モンスターの最大HP</summary>
    protected int maxHP;

    /// <summary>現在のHP（戦闘中に増減）</summary>
    protected int currentHP;

    /// <summary>モンスターの攻撃力(敵モンスターEnemy用)</summary>
    protected int attackPower;

    /// <summary>モンスターの見た目として使用するスプライト</summary>
    protected Sprite monsterImage;

    /// <summary>死亡状態フラグ。true の場合、このモンスターは戦闘不能である</summary>
    protected bool isDead = false;

    /// <summary>ダメージエフェクト1回分の表示時間</summary>
    [SerializeField, Header("ダメージエフェクトの時間")] private float damageEffectTime;

    /// <summary>ダメージ時の点滅回数</summary>
    [SerializeField, Header("エフェクトの点滅回数")] private int blinkCount = 3;

    /// <summary>モンスター画像を表示する UI Image コンポーネント</summary>
    private Image characterImage;

    /// <summary>元の画像カラー（エフェクト終了後に戻すため保持）</summary>
    private Color originalColor;

    /// <summary>ダメージ時に使用する色</summary>
    private Color damageColor = Color.red;

    /// <summary>ハイライト演出用のコルーチン参照</summary>
    private Coroutine highlightCoroutine;

    /// <summary>ハイライト表示時の色</summary>
    private Color highlightColor = Color.blue;

    /// <summary>
    /// 初期化処理。
    /// 子オブジェクトに存在する Image コンポーネントを取得し、
    /// 元のカラーを保存する。
    /// </summary>
    private void Awake()
    {
        characterImage = GetComponentInChildren<Image>();
        if(characterImage != null)
            originalColor = characterImage.color;
    }

    /// <summary>
    /// モンスター共通の初期化処理。
    /// データクラス（MonsterData）の内容を、
    /// 実体モンスターに反映するために使用される。
    /// </summary>
    /// <param name="id">モンスターID</param>
    /// <param name="name">モンスター名</param>
    /// <param name="maxHp">最大HP</param>
    /// <param name="attack">攻撃力</param>
    /// <param name="image">表示用スプライト</param>
    protected void InitializeBase(int id, string name, int maxHp, int attack, Sprite image)
    {
        monsterID = id;
        monsterName = name;
        maxHP = maxHp;
        currentHP = maxHp;
        attackPower = attack;
        monsterImage = image;

        UpdateImage();
    }

    /// <summary>
    /// モンスター画像の表示を更新する。
    /// monsterImage の内容を UI Image に反映する。
    /// </summary>
    protected void UpdateImage()
    {
        if (characterImage != null)
            characterImage.sprite = monsterImage;
    }

    /// <summary>
    /// ダメージを受けた際の視覚エフェクトを再生する。
    /// </summary>
    public void PlayDamageEffect()
    {
        if(characterImage == null) return;

        StartCoroutine(DamageEffectCoroutine());
    }

    /// <summary>
    /// ダメージ時に画像を点滅させるコルーチン。
    /// </summary>
    private IEnumerator DamageEffectCoroutine()
    {
        if(characterImage == null) yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            characterImage.color = damageColor;
            yield return new WaitForSeconds(damageEffectTime);
            
            characterImage.color = originalColor;
            yield return new WaitForSeconds(damageEffectTime);
        }
    }

    /// <summary>
    /// 対象選択時などに使用されるハイライト演出を開始する。
    /// </summary>
    public void StartHighlight()
    {
        if(characterImage == null) return;

        if(highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightCoroutine());
    }

    /// <summary>
    /// ハイライト演出を停止し、元の表示に戻す。
    /// </summary>
    public void StopHighlight()
    {
        if(characterImage == null) return;

        if(highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        characterImage.color = originalColor;
    }

    /// <summary>
    /// ハイライト表示を点滅で表現するためのコルーチン。
    /// </summary>
    private IEnumerator HighlightCoroutine()
    {

        while (true)
        {
            characterImage.color = highlightColor;
            yield return new WaitForSeconds(0.3f);

            characterImage.color = originalColor;
            yield return new WaitForSeconds(0.3f);
        }
    }



    /// <summary>モンスターIDを取得</summary>
    public int GetMonsterID() => monsterID;

    /// <summary>モンスター名を取得</summary>
    public string GetMonsterName() => monsterName;

    /// <summary>最大HPを取得</summary>
    public int GetMaxHP() => maxHP;

    /// <summary>現在HPを取得</summary>
    public int GetCurrentHP() => currentHP;

    /// <summary>攻撃力を取得</summary>
    public int GetAttackPower() => attackPower;

    /// <summary>表示用スプライトを取得</summary>
    public Sprite GetMonsterImage() => monsterImage;

    /// <summary>死亡状態かどうかを取得</summary>
    public bool GetIsDead() => isDead;
}
