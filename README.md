# SSJJ-SkyDome

生死狙击 SkyDome 大挂 - 杜海鹏内部开源

## 项目说明

本项目为 SkyDome（针对《生死狙击》/ SSJJ 的辅助工具）内部开源版本。

## 目录结构

- `Feature/` - 主要功能模块（Legit / Rage / Visuals 等）
  - Aimbot、Triggerbot、Silentbot、AntiAim、ESP、WallHack 等
- `Loader.cs` / `Main.cs` - 入口与加载逻辑
- `MonoMod_Hook/`、`DotNetDetour/` - Hook 相关实现
- `Resources/` - 字体与 vendored MonoMod/SkyCore 等依赖
- `Utilities/` - 工具类
- `scratch/` - 辅助检查/签名工具

## 注意事项

- 本项目仅供学习研究，请勿用于非法用途。
- 构建产物（bin/obj）、个人敏感文件已通过 `.gitignore` 排除。
- 使用前请自行了解风险。

## 构建

使用 Visual Studio 打开 `SkyDome.sln` 进行编译。

---

内部开源 | 仅供参考
