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
### 如果你要在Windows(Server)上部署
- 那么你可以省略```配置账户凭证```中的内容，你只需要构建```QwenAPIGUI```，然后把它上传到服务器，启动后直接输入你的```Email```和```Password```（或者使用Google和其他登录方式），完成后刷新WebView2的页面，然后点击```Auth```中的```WebAPI```按钮，启动WebAPI服务即可。
- [视频演示](https://www.bilibili.com/video/BV1BumHBkE1z/)
