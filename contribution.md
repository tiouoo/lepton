# 贡献指南

## 初始化

### 0. 环境准备

1. Git
2. dotNET 10 SDK
3. Python 3 (仅用于执行脚本)

### 1. 克隆本项目

```bash
git clone https://github.com/tiouoo/lepton.git
cd lepton
```

### 2. 更新子模块

Windows：

```powershell
.\scripts\update-submodule.bat
```

Linux / macOS：

```bash
bash scripts/update-submodule.sh
```

## 注意事项

### 1. LF 换行符

所有文本文件统一使用 LF 换行符。如大量文件换行符改成了 CRLF 等格式，可以执行以下脚本恢复为 LF

Windows：

```powershell
.\scripts\normalize-lf.bat
```

Linux / macOS：

```bash
bash scripts/normalize-lf.sh
```

### 2. 敏感信息

提交前请检查变更内容，确保没有包含个人密钥、API Token、密码、证书私钥等敏感信息
此类信息请通过环境变量或本地配置管理，并将相关本地配置文件加入 `.gitignore`
