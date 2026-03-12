# モンスターカードゲーム

素材を合成し、モンスターを生成して敵と戦闘
<img width="1586" height="902" alt="Image" src="https://github.com/user-attachments/assets/95e5c7d9-90a8-4a23-a222-2a8f5e3af36f" />
図鑑
<img width="1580" height="886" alt="Image" src="https://github.com/user-attachments/assets/dd5f4067-e74d-49ae-a0bb-8e34c5e317bf" />
合成
<img width="1570" height="884" alt="Image" src="https://github.com/user-attachments/assets/924e98fc-ba49-4dc2-9e74-3259fe02b0de" />


### 1. Strategyパターンを用いた敵AIの設計
敵の行動ロジック（攻撃、回復、ガードなど）を `ScriptableObject` として部品化し、Strategyパターンを用いて実装しました。これにより、新しい行動パターンを追加する際に `Enemy` クラス本体を修正する必要がなくなり、保守性と拡張性の高い設計を実現しています。

### 2. 拡張エディタを用いた効率的なデータ管理
モンスターや素材（臓器）などの大量の `ScriptableObject` を一元管理・編集するため、Unityの拡張エディタ（Editor Window）を自作しました。インスペクターを一つずつ開く手間を省き、一覧画面から直接パラメータ調整や新規作成を行えるようにしたことで、データ入力の作業効率を大幅に向上させています。

### 3. 汎用的なUIコンポーネントの開発
インベントリや図鑑のUIにおいて、`GenericSlotUI` という汎用クラスを作成しました。アイテムの所持/未所持（シルエット表示）などの状態をカプセル化し、複数の画面で使い回せるコンポーネント指向なUI実装を行っています。
