# Changelog
## [1.0.33] - 2023-08-28
### Fix
- 修改Split的参数从ReadOnlySpan<char>为char[]，避免隐式转换。

# Changelog
## [1.0.32] - 2023-08-21
### Fix
- 添加路径合并函数组UString.Combine。

# Changelog
## [1.0.31] - 2023-08-21
### Fix
- 富文本例子：支持没有任何规律的合法tag。

# Changelog
## [1.0.30] - 2023-08-21
### Fix
- 富文本例子：修复新版解析没有正确符号的bug。

# Changelog
## [1.0.29] - 2023-08-21
### Fix
- 富文本例子：修复新版解析对<a href=>的支持。

# Changelog
## [1.0.28] - 2023-08-21
### Fix
- 添加对char this[int i]的支持。

# Changelog
## [1.0.27] - 2023-08-21
### Fix
- 富文本例子：修复新版解析对<a href=>的支持。

# Changelog
## [1.0.26] - 2023-08-17
### Fix
- 富文本例子：添加对>、<标记字符做为内容的处理。

# Changelog
## [1.0.25] - 2023-08-15
### Fix
- 富文本例子：修复对空格字符没有过滤的bug。

# Changelog
## [1.0.22] - 2023-08-14
### Fix
- 富文本例子：修复对单字符解析为空的bug。

# Changelog
## [1.0.21] - 2023-08-09
### Fix
- 富文本例子：修复对1个字符解析为空的bug。

# Changelog
## [1.0.20] - 2023-08-08
### Fix
- 富文本例子：EndTag改成HtmlTag数据结构，并传递EndIndex数据。

# Changelog
## [1.0.19] - 2023-08-07
### Fix
- 富文本例子：HtmlTag添加原始数据字段rawData。

# Changelog
## [1.0.18] - 2023-08-02
### Fix
- 补充函数Remove FormatBool IndexOfAny LastIndexOfAny IsNullOrWhiteSpace GetCacheCapacity。
- 优化Intern外部缓存功能，防止重复内存copy。
- 添加默认赋值String.Empty。

# Changelog
## [1.0.17] - 2023-07-28
### Fix
- 优化代码实现。

# Changelog
## [1.0.16] - 2023-07-28
### Fix
- 补充Append<T>、AppendLine<T>函数实现。

# Changelog
## [1.0.15] - 2023-07-27
### Fix
- 修改replace函数可以替换为string.empty。
- 更新富文本标记解析器例子

# Changelog
## [1.0.14] - 2023-07-26
### Fix
- 添加预热函数优化预热过程。

# Changelog
## [1.0.13] - 2023-07-25
### Fix
- 补充IndexOf重载函数。
- 更新富文本标记解析器例子

# Changelog
## [1.0.12] - 2023-07-19
### Fix
- 修复比较运算符逻辑bug
- 更新富文本标记解析器例子

# Changelog
## [1.0.11] - 2023-07-19
### Fix
- 修复比较运算符逻辑bug

# Changelog
## [1.0.10] - 2023-07-18
### Fix
- 增加Yoka.UnityString命名空间

# Changelog
## [1.0.9] - 2023-07-18
### Fix
- 补充一个富文本标记解析器例子

# Changelog
## [1.0.8] - 2023-07-18
### Fix
- 补充带gc的Clone函数，满足某特定场景需求

# Changelog
## [1.0.7] - 2023-07-18
### Fix
- 补充IndexOf(char)、split(char)、substring(index)函数

# Changelog
## [1.0.6] - 2023-07-18
### Fix
- 修复分割字符串函数长度不够bug。
- 修改部分函数的访问权限。

# Changelog
## [1.0.5] - 2023-07-17
### Fix
- 分割字符串函数更新算法。

# Changelog
## [1.0.4] - 2023-07-14
### Fix
- 增加部分函数实现。

# Changelog
## [1.0.3] - 2023-07-13
### Fix
- 增加PadLeft、PadRight函数实现。

# Changelog
## [1.0.2] - 2023-07-13
### Fix
- 增加外部缓存模式。

# Changelog
## [1.0.1] - 2023-07-13
### Fix
- 移除不必要的DLL文件。

## [1.0.0] - 2023-07-13
### Added
- Zero Allocation String for Unity.