# 激光粒度分析仪 - Unity交互动画项目

## 项目概述

这是一个高度仿真的激光粒度分析仪（Laser Particle Analyzer）在Unity中的3D交互动画项目。基于Malvern Mastersizer系列工业设备，实现了完整的机械结构、光学路径模拟和实时数据反馈。

### 核心特性
- ✅ 高保真3D仪器模型（总三角面 < 50k）
- ✅ 实时光学路径与散射效果模拟
- ✅ 完整的粒度测量流程动画
- ✅ 专业级数据可视化（粒度分布曲线、D10/D50/D90）
- ✅ 直观的交互式UI控制面板
- ✅ 工业级后处理效果（Bloom、HDR、SMAA）

## 技术栈

- **Unity**: 2022.3.20f1 LTS
- **渲染管线**: Universal Render Pipeline (URP) 14.0.8
- **编程语言**: C#
- **目标平台**: Windows Standalone (PC)
- **性能目标**: 稳定 60 FPS

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── MeasurementController.cs       # 主控制器，管理状态机
│   │   ├── OpticalSimulator.cs            # 光束与散射模拟
│   │   └── DataProcessor.cs               # 粒度计算
│   ├── Models/
│   │   ├── ParticleData.cs                # 单个颗粒数据类
│   │   └── DistributionPreset.cs          # 分布预设枚举与数据
│   ├── UI/
│   │   ├── UIManager.cs                   # 绑定UI控件事件
│   │   ├── ChartDrawer.cs                 # 绘制直方图
│   │   └── DetectorDisplay.cs             # 环形探测器着色
│   ├── Interaction/
│   │   ├── CameraController.cs            # 轨道摄像机
│   │   └── Button3D.cs                    # 物理按键交互
│   └── Utilities/
│       ├── ObjectPool.cs                  # 颗粒对象池
│       └── MathHelper.cs                  # 数学工具函数
├── Prefabs/                               # 预制体
├── Materials/                             # URP Lit材质
├── Audio/                                 # 音效资源
├── Textures/                              # 纹理资源
└── Scenes/
    └── MainScene.unity                    # 主场景
```

## 快速开始

### 1. 环境配置

- 安装 Unity 2022.3.20f1 LTS
- 通过 Unity Hub 打开此项目
- 首次打开时会自动下载并安装 URP 依赖

### 2. 初始化资源

项目包含自动资源生成脚本。首次运行场景时，系统会自动：n- 生成程序化纹理（红、蓝、灰色等）
- 创建合成音频片段（激光启动音、完成提示音）
- 初始化材质和预制体

### 3. 运行场景

1. 打开 `Assets/Scenes/MainScene.unity`
2. 按 Play 键开始
3. 使用以下控制方式：
   - **鼠标左键**: 点击按钮、点击并拖动旋转视角
   - **鼠标滚轮**: 缩放视角
   - **Space**: 开始/暂停测量
   - **R**: 重置所有参数
   - **F**: 全屏切换

## 核心模块说明

### MeasurementController (主控制器)

有限状态机实现，管理测量流程的各个阶段：
- `Idle`: 待机状态
- `WarmingUp`: 激光预热（0~0.5s）
- `Measuring`: 测量进行中（0.5~7.0s）
- `Processing`: 数据处理（7.0~8.0s）
- `Complete`: 测量完成

**暴露的公共方法：**
```csharp
void StartMeasurement()                    // 开始测量
void PauseResume()                         // 暂停/继续
void ResetAll()                            // 重置系统
void SetContinuousMode(bool enable)        // 连续测量模式
void SetConcentration(float percent)       // 设置浓度 (0-100%)
void SetDistributionPreset(int index)      // 设置粒径分布预设
```

### OpticalSimulator (光学模拟器)

模拟激光束路径和散射效果：
- 光束生成与传播
- 样品池内颗粒散射
- 探测器响应计算

**暴露的公共方法：**
```csharp
void SetLaserIntensity(float intensity)           // 设置激光强度 (0-1)
void UpdateBeamExtension(float progress)          // 更新光束延伸 (0-1)
void UpdateScattering(float progress)             // 更新散射效果 (0-1)
void UpdateDetectorResponse(float progress)       // 更新探测器响应 (0-1)
float[] GetDetectorIntensities()                  // 获取探测器数据
void Reset()                                       // 重置光学效果
```

### DataProcessor (数据处理器)

粒度数据计算和统计：
- 粒径分布生成
- D10/D50/D90 计算
- 比表面积推算

**暴露的公共方法：**
```csharp
void SetDistributionType(DistributionType type)  // 设置分布类型
void SetConcentration(float concentration)        // 设置浓度 (0-1)
void ProcessMeasurementData()                     // 处理测量数据
MeasurementResult GetResults()                    // 获取结果
void Reset()                                      // 重置处理器
```

### UIManager & ChartDrawer

实时数据可视化：
- 粒度分布直方图绘制
- 关键指标显示（D10、D50、D90、SSA）
- 探测器扫描环形图

## 可视化参数配置

在 Inspector 中调整以下参数：

### MeasurementController
- `laserIntensity`: 激光强度 (0-1, 推荐1.0)
- `measurementDuration`: 单次测量时长 (秒, 推荐7.0)
- `continuousMeasurementInterval`: 连续模式间隔 (秒, 推荐5.0)

### OpticalSimulator
- `beamWidth`: 光束宽度 (米, 推荐0.008)
- `beamLength`: 光束长度 (米, 推荐1.0)
- `scatterParticleCount`: 散射粒子最大数 (推荐150)
- `detectorSectorCount`: 探测器扇区数 (推荐36)
- `detectorRadius`: 探测器半径 (米, 推荐0.1)

### DataProcessor
- `particlePoolCapacity`: 对象池容量 (推荐200)
- `densityRatio`: 颗粒密度 (g/cm³, 推荐2.65)

## 粒径分布预设

系统提供4种标准分布模式：

1. **单分散** (`Monodisperse`)
   - 所有颗粒粒径相同: 10 ± 0.5 μm
   - 用于对照实验

2. **双峰分布** (`Bimodal`)
   - 40% @ 5 μm + 60% @ 20 μm
   - 混合样品常见

3. **宽分布** (`WideDistribution`)
   - 对数正态分布: μ=15 μm, σ=0.5
   - 范围 1~50 μm
   - 实际样品常见

4. **罗辛-拉姆勒分布** (`RosinRammler`)
   - 工业筛分标准分布
   - D50=12 μm, 形状参数n=2.0

## 关键参数说明

### 测量流程时序

| 阶段 | 时间范围 | 描述 |
|------|---------|------|
| 预热 | 0~0.5s | 激光逐渐启动 |
| 出射 | 0.5~2.0s | 光束从发射器向前延伸 |
| 散射 | 2.0~3.5s | 光束穿过样品池，颗粒散射 |
| 汇聚 | 3.5~5.0s | 光束经傅里叶透镜聚焦 |
| 采集 | 5.0~6.0s | 探测器采集光强数据 |
| 显示 | 6.0~7.0s | 绘制结果曲线 |

## 常见问题

### Q: 首次打开报错 "URP Pipeline Asset not found"？
**A:** 场景初始化脚本会自动创建 URP 管线资产。若未自动创建，请手动执行菜单 `Assets > Create > Rendering > URP Asset`，然后在 Project Settings > Graphics 中指定。

### Q: 颗粒性能不足，帧率下降？
**A:** 
1. 减少 `scatterParticleCount` 参数
2. 在 Quality Settings 中降低质量等级
3. 禁用颗粒的阴影投射
4. 使用 Profiler 检查性能瓶颈

### Q: 音效无法播放？
**A:** 
1. 检查 Main Camera 是否有 AudioListener 组件
2. 确认 AudioClip 正确指派
3. 在 Audio Settings 中验证扬声器配置

### Q: 如何修改仪器外观？
**A:** 编辑 `OpticalSimulator.cs` 中的几何体参数，或在场景中直接修改 Inspector 数值。

## 资源替换指南

### 贴图替换

所有贴图初始为程序生成的纯色，保存在 `Assets/Textures/Generated/` 目录。

**替换步骤：**
1. 将自定义贴图导入 `Assets/Textures/`
2. 在对应 Material 中更换纹理引用
3. 调整 Metallic、Roughness、Normal Map 参数

### 音效替换

音效初始为合成的正弦波，保存在 `Assets/Audio/Generated/` 目录。

**替换步骤：**
1. 将 .wav 或 .ogg 音频文件导入 `Assets/Audio/`
2. 在代码中修改 AudioClip 引用
3. 调整音量和播放时机参数

## 性能优化建议

1. **对象池**: 已启用颗粒对象池，减少 GC 开销
2. **批处理**: 启用 GPU Instancing，合并材质
3. **阴影**: 禁用颗粒的阴影投射
4. **粒子系统**: 使用脚本控制 Emission 而非预设
5. **UI**: 仅在数据更新时重新生成图表纹理
6. **后处理**: 根据目标平台调整 Bloom 和抗锯齿强度

## 扩展开发

### 添加新的粒径分布预设

编辑 `DataProcessor.cs` 中的 `DistributionType` 枚举，新增类型并实现对应的生成逻辑。

### 自定义探测器配置

在 `OpticalSimulator.cs` 中修改 `detectorSectorCount` 和角度范围。

### 集成真实仪器数据

在 `DataProcessor.cs` 中添加数据导入接口，支持 CSV 或 JSON 格式。

## 许可证

MIT License

## 联系方式

项目维护者: dhyhxy

---

**最后更新**: 2026-06-11  
**项目版本**: 1.0.0  
**Unity LTS 版本**: 2022.3.20f1
