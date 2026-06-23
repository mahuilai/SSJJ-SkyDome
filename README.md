# SkyDome大挂源代码(清理修复版)

> 已修复闪退问题

>内附注入器源码

> 已清理垃圾无用代码

---

## 项目概述

SkyDome 是一个完整的游戏内部作弊框架，以 **DLL 注入** 方式嵌入 Unity IL2CPP 游戏进程运行时。该项目展示了 C# 在游戏内存操作、运行时 Hook、反检测对抗方面的深度技术实践。

| 项目属性 | 内容 |
|---------|------|
| 目标平台 | Windows x64 |
| 目标框架 | .NET Framework 4.8 |
| 注入方式 | SharpMonoInjector (Mono 注入) |
| 运行时 | Unity IL2CPP + Mono |
| 混淆工具 | ConfuserEx (Release 构建) |

---

## 架构总览

```
SkyDome.dll (注入入口)
├── SceneInitializer.Load()              # 加载入口：反检测安装 → 程序集加载 → 组件注册
│   ├── GetAssembliesBypass.Install()     # NativeDetour: AppDomain.GetAssemblies 过滤
│   ├── MonoBypass.Install()              # MethodHook: 额外枚举 API 过滤
│   ├── Main.LoadAllAssembly()            # 从嵌入资源加载 MonoMod/Cecil 程序集
│   └── Main.Awake() → Init()            # 创建 HookObject → 注册所有 Unity 组件
│
├── HookEngine (12 个游戏函数钩子)        # Hook 游戏相机/朝向系统实现功能
│   ├── TpsCameraLogic (IsActive/Update)  # 第三人称实现
│   ├── CameraFunction (GetCmdYaw/Pitch) # 视角强制同步
│   ├── UiIEventCondition (Yaw)          # UI 朝向同步
│   ├── CommandsComponent (LastYaw/Pitch)# UserCmd 数据拦截
│   ├── PlayerEntity.get_fov             # FOV 修改
│   └── PlayerOrientationSystem (3个)    # 朝向预测/回放 Hook (Silent Aim)
│
├── Feature/                            # 功能模块 (30+ 组件)
│   ├── Legit/                          # 自瞄、自动扳机
│   ├── Rage/                           # Silent Aim、Anti-Aim
│   ├── Visuals/                        # ESP、雷达、Chams 等
│   └── ...
│
├── DotNetDetour/                       # 跨平台 IL2CPP Hook 引擎
├── Cfg/                                # 配置持久化 (Key-Value 文件系统)
├── Render/                             # Unity GL/IMGUI 渲染系统
├── Utilities/                          # 反检测 + 工具函数
└── AutoInjector/                       # 自动注入器 (进程监控/自动注入)
```

---

## 功能清单

### 战斗 (Legit)

| 功能 | 类名 | 说明 |
|------|------|------|
| 自瞄 | `TargetSelector` | 支持23个身体部位（头/颈/脊椎/四肢/手指/脚趾等） |
| | | FOV 限制 + 可见性检测 |
| | | 平滑自瞄（轴偏移量注入） |
| | | 弹道散布预测 |
| 自动扳机 | `AutoFireController` | 准星指向敌人自动开枪 |
| | | 狙击枪延迟激活 |
| | | 散布预测模式 |

### 暴力 (Rage)

| 功能 | 类名 | 说明 |
|------|------|------|
| Silent Aim | `ShotCalculator` + `AngleCorrector` | 劫持 UserCmd 视角实现无感知自瞄 |
| | | 每骨骼检测 + 散布偏移计算 |
| | | 精准度阈值控制 |
| Anti-Aim | `AngleCorrector` | 视角扭曲（Jitter/Spin） |
| | | 移动修正 |

### 视觉 (Visuals)

| 功能 | 类名 | 说明 |
|------|------|------|
| 方框透视 | `EntityVisualizer` | 2D Box / Corner Box / 3D Box |
| 骨骼透视 | `OverlayHost.DrawSkeleton()` | 完整 Bip01 骨骼连线 + 头部圆形标记 |
| 血量条 | `OverlayHost.DrawVerticalHealthBar` | 颜色渐变（红→绿） |
| 雷达 | `MiniMapOverlay` | 屏幕圆形小地图（彩虹风格） |
| Chams | `MaterialOverlay` | 基于 OutlineEffect 的轮廓高亮 |
| 物品 ESP | `PickupHighlighter` | 显示掉落武器名称/距离 |
| 特效 ESP | `SceneBuffESP` / `DynamicEntityESP` | 场景增益/动态实体标记 |
| C4 计时器 | `BombTimer` | C4 爆炸倒计时显示 |
| 观察者列表 | `SpectatorList` | 显示正在观察自己的玩家 |
| 自定义准星 | `ReticleRenderer` | 可配置准星样式 |
| 射线追踪 | `Trace` | 弹道激光线 |
| 命中点标记 | `ImpactPoint` | 子弹命中点绘制 |
| 反自瞄指示器 | `AntiAimIndicator` | 显示当前 Anti-Aim 状态 |
| 信息显示 | `FeatureIndicator` / `StatusDisplay` | 功能开关状态/玩家信息叠加 |

### 其他

| 功能 | 说明 |
|------|------|
| 无后座力 | `ViewStabilizer` / `PitchSynchronizer` |
| 第三人称 | Hook TpsCameraLogic 实现 |
| 自定义 FOV | 第一/第三人称独立 FOV 设置 |
| Bunny Hop | 自动连跳 |
| 皮肤更换 | `SkinChanger` |
| 世界设置 | `WorldSettings`（天气/时间等） |
| Fake Lag | 网络延迟伪造 |
| No Flash | 防闪光弹 |
| 消息发送 | `Say`（游戏内消息） |

---

## 反检测系统

SkyDome 实现了多层反检测机制：

### 第1层：程序集隐藏

| 技术 | 实现 | 说明 |
|------|------|------|
| `AppDomain.GetAssemblies` Hook | `NativeDetour` | 原生 icall Hook，过滤 SkyDome/SkyCore/MonoMod/Cecil/DotNetDetour |
| `Assembly.GetTypes` Hook | `MethodHook` | 防止反作弊枚举程序集类型 |
| `Module.GetTypes` Hook | `MethodHook` | 同上，模块级别 |
| `Assembly.GetExportedTypes` Hook | `MethodHook` | 类型枚举过滤 |
| `Assembly.GetLoadedModules` Hook | `MethodHook` | 模块枚举过滤 |
| `Assembly.GetModules` Hook | `MethodHook` | 模块枚举过滤 |
| `Assembly.GetReferencedAssemblies` Hook | `MethodHook` | 引用程序集过滤 |
| `Assembly.CreateInstance` Hook | `MethodHook` | 阻止通过名称创建敏感类型实例 |

### 第2层：程序集加载隐藏

| 技术 | 实现 | 说明 |
|------|------|------|
| 程序集重命名 | MonoMod.Utils → SkyDome.Utils | 避免特征名称暴露 |
| | MonoMod.RuntimeDetour → SkyDome.CoreRuntime | 同上 |
| 资源嵌入加载 | `Assembly.Load(byte[])` | 不从磁盘加载，无文件特征 |
| AssemblyResolve 过滤 | `ResolveHelper` | 仅允许 SkyDome 自身代码解析敏感程序集 |
| | | 非授信请求返回 null |
| | | 兜底: StackTrace 调用栈检查 |

### 第3层：运行时反检测

| 技术 | 实现位置 | 说明 |
|------|---------|------|
| PE 头擦除 | `ErasePEHeaders()` | 擦除 DOS 头 + PE 签名（512字节） |
| 节区名覆盖 | `OverwriteSectionNames()` | 随机 ASCII 覆盖所有节区名 |
| 导入目录擦除 | `OverwriteImportDirectory()` | 清零导入表 RVA/Size |
| RWX → RX | `FixRWXPages()` | 扫描所有 RWX 私有内存页移除写权限；后台线程持续扫描 |
| 垃圾内存分配 | `AllocateJunkMemory()` | 20-50 块随机大小（4-64KB）垃圾内存混淆特征 |
| ConfuserEx 混淆 | 构建后处理 | Release 模式自动执行 |

### 第4层：注入阶段反检测

| 技术 | 说明 |
|------|------|
| `RandomizeMemory = true` | SharpMonoInjector 随机化内存分配 |
| `HideThreads = true` | 隐藏注入线程 |
| Stealth 模式 | 自动隐藏控制台窗口 |
| 远程调试器检测 | `CheckRemoteDebuggerPresent` |
| Mono 就绪检查 | 等待 Mono 运行时初始化完成 |

---

## Hook 技术详解

### DotNetDetour (MethodHook)

跨平台 IL 方法 Hook 引擎，通过运行时修改方法头部字节码实现：

- **x86_64**: 14 字节跳转 (`FF 25` + 地址)
- **x86**: 6 字节跳转 (`68` + 地址 + `C3`)
- **ARM32 (ARM)**: 8 字节 (`LDR PC, [PC, #-4]` + 地址)
- **ARM32 (Thumb)**: 38 字节复合跳板
- **ARM64**: 12 字节 (`LDR PC, [PC, #-4]` + 地址)

支持 **trampoline 生成**：拷贝原始指令 + 跳回原始函数，保留对原函数的完整调用能力。

### NativeDetour

用于 Hook 无 IL Body 的 Mono icall 方法（如 `AppDomain.GetAssemblies`），直接在原生代码层面操作。

### Hook 流程

```
Install():
  1. GetFunctionAddr() → 获取方法原生地址（处理 IL2CPP 特有问题）
  2. InitProxyBuff()  → 计算需覆盖的最小指令长度
  3. BackupHeader()   → 备份原始指令
  4. PatchTargetMethod() → 写入跳转指令 → 替换为 Hook 函数
  5. PatchProxyMethod()  → 可选：代理函数写入原始指令
```

### Undo/Redo 模式

所有 Hook 实现了安全的回退机制：当 trampoline 不可用时，自动采用 `Undo → 调用原始 → Redo` 策略，确保功能稳健性。

---

## 渲染系统

### FastRenderer (GL Immediate Mode)

- Unity `GL` API 实现的高性能 2D 渲染
- 内置材质：`Hidden/Internal-Colored` Shader
- 支持：线条 / 矩形 / 圆形 / 扇形 / 多边形 / 三角形 / 箭头 / 十字准星
- 支持：空心 / 填充两种模式
- 内置 Rainbow HSV 渐变色算法

### OverlayHost (骨骼与血条)

- `DrawSkeleton()` — 完整 Bip01 骨骼连线（脊椎→四肢→手指→头部圆形标记）
- `DrawVerticalHealthBar()` — 纵向血量条（背景 → 颜色渐变填充 → 边框）

### GUI 菜单

- Neverlose 风格 UI（侧边栏 + 内容区布局）
- 6 个标签页：**战斗 / 视觉 / 玩家 / 世界 / 杂项 / 配置**
- 搜索过滤 + 滚动视图
- 彩虹主题配色

---

## 配置系统

- 基于反射的 Key-Value 配置文件存储
- 所有 `SettingsStore` 静态字段自动序列化/反序列化
- 支持类型：`bool`, `int`, `float`, `string`, `KeyCode`
- 配置文件路径：`Application.persistentDataPath/SkySettingsHelper/`
- 多配置切换（保存 / 加载 / 删除）

---

## 注入方式

### AutoInjector

SkyDome 提供了独立的自动注入器 `AutoInjector.exe`：

```
AutoInjector.exe [-stealth] [-nowait]
```

- **进程监控**：持续扫描 `SSJJ_BattleClient_Unity` 进程
- **Mono 就绪检测**：验证 Mono 运行时已初始化
- **自动注入**：检测到新进程 → 等待 800ms → 注入
- **多进程支持**：追踪已注入 PID，避免重复注入
- **Stealth 模式**：`-stealth` 隐藏控制台窗口
- **No-wait 模式**：`-nowait` 注入成功后 5 秒自动退出

### 注入参数

```
命名空间: SkyDome
类:       SceneInitializer
方法:     Load (静态, 无参数)
```

---

## 构建说明

### 前置要求

- Visual Studio 2019+ / MSBuild 15.0+
- .NET Framework 4.8 SDK
- ConfuserEx (可选，Release 混淆用，路径：`D:\Compressed\ConfuserEx-CLI\Confuser.CLI.exe`)

### 构建命令

```bash
# Debug (开发测试)
msbuild SkyDome.csproj /p:Configuration=Debug

# Release (含 ConfuserEx 混淆)
msbuild SkyDome.csproj /p:Configuration=Release
```

### 项目依赖

**外部依赖**（游戏相关程序集，需自行引用）：
- `Assembly-CSharp.dll` — 游戏主程序集
- `SSJJMath_Library.dll` / `SSJJPhysics_Library.dll` — 数学/物理库
- `Entitas.dll` — ECS 框架
- `UnityEngine.CoreModule.dll` + 各子模块
- 以及 `Newtonsoft.Json`, `protobuf-net`, `DOTween` 等第三方库

**内部依赖**（嵌入资源，已包含在项目中）：

| 资源 | 原始名称 | 重命名后 |
|------|---------|---------|
| MonoMod.RuntimeDetour | MonoMod.RuntimeDetour.dll | SkyDome.CoreRuntime.dll |
| MonoMod.Utils | MonoMod.Utils.dll | SkyDome.Utils.dll |
| Mono.Cecil | Mono.Cecil.dll | Mono.Cecil.dll |

---

## 项目文件结构

```
SkyDome/
├── Main.cs                       # 入口 MonoBehaviour，注册所有 Hook 组件
├── Loader.cs                     # SceneInitializer + GetAssembliesBypass
├── GlobalEvents.cs               # 事件广播 (OnPlayerHit)
├── Cfg/
│   ├── Config.cs                 # SettingsStore 配置定义 (~80 个开关/参数)
│   ├── ConfigManager.cs          # 配置持久化读写
│   └── Menu.cs                   # GUI 菜单系统
├── Engine/
│   ├── InputDriver.cs            # 输入驱动 (键鼠强制注入)
│   └── MouseSimulator.cs         # 鼠标模拟
├── Entity/
│   ├── PlayerInfo.cs             # PlayerData 封装 (游戏实体属性)
│   └── PlayerUpdate.cs           # PlayerStateTracker (本地/实体列表追踪)
├── Feature/
│   ├── Legit/Aimbot.cs           # TargetSelector (自瞄)
│   ├── Legit/Triggerbot.cs       # AutoFireController (自动扳机)
│   ├── Rage/AntiAim.cs           # AngleCorrector (Anti-Aim + Silent Aim 控制器)
│   ├── Rage/Silentbot.cs         # ShotCalculator (精确弹道计算/骨骼瞄准)
│   ├── Visuals/WallHack.cs       # MiniMapOverlay (圆形雷达)
│   ├── Visuals/Chams.cs          # MaterialOverlay (Outline 轮廓高亮)
│   ├── Visuals/BoundingBox3D.cs  # EntityBoxRenderer (3D Box)
│   ├── Visuals/Radar.cs          # Radar 组件
│   ├── Visuals/ItemESP.cs        # PickupHighlighter (掉落物高亮)
│   ├── Visuals/C4Timer.cs        # BombTimer (C4计时)
│   ├── Visuals/Crosshair.cs      # ReticleRenderer (自定义准星)
│   ├── Visuals/Trace.cs          # 射线追踪
│   ├── Visuals/SpectatorList.cs  # 观察者列表
│   ├── Visuals/AntiAimIndicator.cs   # Anti-Aim 状态指示器
│   ├── Visuals/FeatureIndicator.cs   # 功能状态指示器
│   ├── Visuals/SceneBuffESP.cs       # 场景增益 ESP
│   ├── Visuals/MoveEntityESP.cs      # 移动实体 ESP
│   ├── Visuals/ItemOutline.cs        # 物品轮廓
│   ├── Resolver.cs               # PitchSynchronizer (俯仰同步/Resolver)
│   ├── NoRecoil.cs               # ViewStabilizer (视角稳定器)
│   ├── SkinChanger.cs            # 皮肤更换
│   ├── WorldSettings.cs          # 世界设置
│   ├── Say.cs                    # 游戏内消息
│   ├── Stubs.cs                  # 空桩组件 (AutoSheath/AutoDance/HealthDisplay)
│   └── AutoTrigger/              # 自动触发 (WindSpirit/AutoRecall)
├── HookEngine.cs                 # 12个游戏函数 Hook (相机/朝向/UserCmd)
├── MonoMod_Hook/
│   ├── HookManager.cs            # Hook 注册/管理
│   └── MethodHook.cs             # MonoMod 方式 MethodHook
├── Render/
│   ├── ImmediateRenderer.cs      # FastRenderer (GL Immediate Mode 渲染)
│   └── OverLay.cs                # OverlayHost (骨骼/血条绘制)
├── Utilities/
│   ├── AntiDetection.cs          # RuntimeProtection (PE擦除/内存清理/垃圾分配)
│   ├── MonoBypass.cs             # 额外枚举 API Hook
│   ├── PathRendererHelper.cs     # 弹道检测/射线追踪 (反射调用 FireUtility)
│   ├── PlayerUtility.cs          # Transform 查询/骨骼节点工具
│   ├── MathUtility.cs            # 数学工具
│   ├── ViewportUtility.cs        # 视口坐标转换
│   ├── StringCipher.cs           # 字符串加密
│   └── ReflectionExtensions.cs   # 反射扩展
├── AutoInjector/                 # 自动注入器 (监控进程 + 注入)
├── DotNetDetour/                 # 跨平台 Hook 引擎
│   ├── MethodHook.cs             # 核心 Hook 实现
│   ├── HookPool.cs               # Hook 对象池
│   ├── IL2CPPHelper.cs           # IL2CPP 内存操作 (VirtualProtect)
│   └── LDasm.cs                  # 指令长度反汇编引擎
├── Properties/                   # 程序集属性/资源
├── Resources/                    # 嵌入资源 (字体/程序集)
└── scratch/                      # 辅助工具/签名检查
```

---

## 技术亮点

1. **NativeDetour for icall** — 使用原生 Hook 技术劫持 `AppDomain.GetAssemblies`，这是常规 .NET Hook 无法做到的（icall 没有 IL Body）

2. **多层安全过滤** — AssemblyResolve 同时校验 `RequestingAssembly` 和 `StackTrace`（反射调用时前者为 null），杜绝反作弊绕过

3. **Undo/Redo 回退** — 所有 Hook 同时支持 trampoline 和 Undo/Redo 两种执行路径，trampoline 失败时自动降级

4. **彩虹渲染引擎** — `FastRenderer` 内置 HSV 彩虹渐变算法，所有视觉效果支持动态彩虹配色

5. **精确弹道模拟** — `ShotCalculator` 使用游戏同源的 `FireUtility.CalShotsFiredSpread` + `UniformRandom` 进行散布预测和弹道补偿

6. **全骨骼追踪** — 完整 Bip01 骨架系统（41 个骨骼点），支持 23 个瞄准部位选择和 3D Bounding Box 计算

7. **输入驱动劫持** — `InputDriver` 实现了完整的 `IDeviceInput` 接口替换方案，支持强制按键/鼠标/轴状态注入

---

## 免责声明

本项目为 **学习研究目的** 开源，仅供研究 Unity IL2CPP 运行时 Hook 技术、.NET 内存操作和反检测对抗技术使用。使用者应遵守相关法律法规及游戏服务条款，禁止在未经授权的环境中使用。

作者不对使用本项目产生的任何后果承担责任。

---


### 依赖目标游戏

通过命名空间和引用推断目标为 **SSJJ**（生死突击/生死狙击类）的 Unity 手游/端游，使用 IL2CPP 运行时（代码中有大量 `IsIL2CPP()` 判断），引用包括 `SSJJMath`、`Assets.Sources.Utils.Weapon`、`share`、`NetData` 等游戏内部程序集。

## 注意事项

- 本项目仅供学习研究，请勿用于非法用途。
- 构建产物（bin/obj）、个人敏感文件已通过 `.gitignore` 排除。

## 构建

使用 Visual Studio 打开 `SkyDome.sln` 进行编译。

仅供参考
