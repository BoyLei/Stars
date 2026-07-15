# ServiceModule<T>

**继承：** `ServiceModule<T> : Module`  where T : ServiceModule<T>, new()
**文件：** `Assets/Scripts/StarFramework/Module/Framework/ServiceModule.cs`

## 职责

泛型单例服务基类，所有常驻服务继承自此。

## 特性

- 泛型单例模式：`Instance` 属性提供全局访问
- 自动 `CheckSingleton()` 确保单例约束
- 与普通模块统一生命周期管理

## 使用示例

```csharp
public class NetworkManager : ServiceModule<NetworkManager>
{
    public static NetworkManager Instance => GetInstance();
}
```
