# Yoka Asset Checker Toolset
## 素材检查器工具集

### Yoka AssetChecker
- ShaderAssetChecker 
  - 可以检查项目的Shader复杂度、Shader里面耗时函数的提示、编译后采样函数的统计，并生成HTML报告。
  - 导入package到项目后，首先需要在合适的位置单击右键Create/ShaderCheckerDefine，创建配置文件，配置选项。
  - 功能基于Arm Mobile Studio/mali_offline_compiler，需要提前下载安装，<a href="https://developer.arm.com/Tools%20and%20Software/Mali%20Offline%20Compiler">下载链接</a>
  - <img src='./screenshots/image_01.jpg'/>
  - <a href="Editor/说明文档/ShaderAssetChecker.pdf">使用说明文档链接</a>。

- AssetBundlerChecker
  - 可以检查项目的AB依赖引用关系、AB的关系链功能、循环依赖查询功能等。
  - <a href="Editor/说明文档/AssetBundleChecker.pdf">使用说明文档链接</a>
