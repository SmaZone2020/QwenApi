# QwenApi

## 配置账户凭证
- 运行一次项目后，将在可执行文件下生成一个`config`文件夹，其中有一个`config.txt`文件，文件格式为：
```
bx-ua
cookie
bx-umidtoken
```
- 这些数据需通过已登陆的Qwen网页版的`/api/v2/chat/completions`接口的请求体中获取。