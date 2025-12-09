# QwenApi

## 配置账户凭证
- 运行一次项目后，将在可执行文件下生成一个`config`文件夹，其中有一个`config.txt`文件，文件格式为：
```
bx-ua
cookie
bx-umidtoken
```
- 这些数据需通过已登陆的Qwen网页版的`/api/v2/chat/completions`接口的请求体中获取。

## QwenWebAPI部署配置指南
- 除了和上面一样的配置方法外，还要额外设置```QwenWebAPI/Controllers/MainController.cs:21```的`expected`字段，这个字段仅用于防止恶意请求，并且硬编码于代码中，用户可根据自己需求来更改验证方式
