using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildClass
{
    public static void Build()
    {
        // ビルド対象シーンリスト
        string[] sceneList = {
            "./Assets/Scenes/RL-Scene.unity"
        };


        // 実行
        var buildReport = BuildPipeline.BuildPlayer(
                sceneList,                          //!< ビルド対象シーンリスト
                "../build/bipedal.x86_64",   //!< 出力先
                BuildTarget.StandaloneLinux64,      //!< ビルド対象プラットフォーム
                BuildOptions.None            //!< ビルドオプション
        );


        // 結果出力
        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            const int kSuccessCode = 0;
            EditorApplication.Exit(kSuccessCode);
        }
        else
        {
            const int kErrorCode = 1;
            EditorApplication.Exit(kErrorCode);
        }
    }
}
