# UnityBoyomichanClient

UnityBoyomichanClientは、[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)をUnityから操作するためのライブラリです。  
TCP Socketを用いて通信を行います。

# 動作環境

* .NET 4.x以降が利用可能なUnity

# 機能一覧

 * テキスト読み上げ
 * 一時停止
 * 一時停止解除
 * 読み上げスキップ
 * 残りタスク全消去
 * 一時停止状態の取得
 * 残タスク取得
 * 読み上げ状態取得

# サンプル

```cs
private async Task SpeechSample()
{
    var _boyomiClient = new BoyomichanClient("127.0.0.1", 50001);

    await _boyomiClient.TalkAsync(
        message: "これはサンプルです",
        speed: -1, // -1で棒読みちゃん側の設定
        volume: -1,// -1で棒読みちゃん側の設定
        pitch: -1, // -1で棒読みちゃん側の設定
        voiceType: VoiceType.DefaultVoice,
        cancellationToken: default(CancellationToken));

    // 読み上げが終わるのをまつ
    while (await _boyomiClient.CheckNowPlaying())
    {
        await Task.Delay(TimeSpan.FromSeconds(0.5f)); //0.5sごとに確認
    }

    Debug.Log("Done");
}
```


# 配布ライセンス

MIT

