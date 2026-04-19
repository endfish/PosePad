# PosePad

[English](README.md) | [中文说明](README.zh-CN.md)

PosePad は、Final Fantasy XIV 向けの Dalamud プラグインです。よく使うポーズ、エモート、表情をボタンパネルとしてまとめ、GPose、スクリーンショット、演出、ロールプレイの場面で素早く使えるようにします。

## 主な機能

- よく使うポーズのボタン一覧
- ゲーム内データから読み込むエモートタブ
- エモートと分離した表情タブ
- エモートと表情のお気に入り
- 検索とフィルタ
- 最近使ったエモート一覧
- 既定状態へ戻す「動作を解除」ボタン
- GPose でも使いやすい UI 表示と自動オープン設定
- 英語・中国語・日本語 UI

## コマンド

- `/posepad`
- `/posspad`

## 現在のスコープ

現在のバージョンに含まれるもの：

- ローカル用のポーズ／エモートパネル
- ローカル設定の保存
- GPose 中でも比較的安全な timeline 動作再生

現在のバージョンに含まれないもの：

- Penumbra の動作 Mod 自動検出
- Mod とポーズの自動マッピング
- Penumbra への実行時依存
- Brio への実行時依存

## ローカル開発

現在のローカル開発基準：

- `Dalamud.NET.Sdk 14.0.0`
- Dalamud API `14`
- 既定の hooks パス: `$(AppData)\XIVLauncherCN\addon\Hooks\dev`

ビルド:

```powershell
dotnet build
dotnet build -c Release
```

Release の配布用成果物:

```text
bin/Release/PosePad/latest.zip
```

## Dalamud 開発プラグインとして読み込む方法

1. `/xlsettings` を開く
2. `Experimental` を開く
3. `bin/Debug/PosePad.dll` のフルパスを追加する
4. `/xlplugins` を開く
5. 開発プラグイン一覧から PosePad を有効化する
