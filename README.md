# UnityBoyomichanClient

UnityBoyomichanClientは、[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)をUnityから操作するためのライブラリです。  
TCP SocketまたはHTTPを用いて通信を行います。

# 動作環境

* Unity 2022 LTS以降が推奨
* [UniTask](https://github.com/Cysharp/UniTask)


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
    // TCP版
    var _boyomiClient = new TcpBoyomichanClient("127.0.0.1", 50001);

    // HTTP版
    // var _boyomiClient = new HttpBoyomichanClient("localhost", 50080);

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
    _boyomiClient.Dispose();
}
```


# 配布ライセンス

MIT


# 使用ライブラリライセンス表記

### UniTask

The MIT License (MIT)

Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.