# QwenWebAPI 接口使用与请求文档

## 基础信息

- **基础路径**：`/Main`（由控制器路由`[Route("[controller]")]`定义，控制器名为`MainController`）
- **认证方式**：通过请求头`Auth`进行验证，需与`MainController.cs`中硬编码的`expected`字段匹配
- **数据格式**：请求与响应均使用JSON格式
- **编码格式**：UTF-8

## 认证说明

所有接口均需要在请求头中包含有效的`Auth`字段，格式如下：
```http
Auth: {你的认证密钥}
```
> 密钥位置：`QwenWebAPI/Controllers/MainController.cs:21`中的`expected`字段，可根据需求修改验证方式

## 接口详情

### 1. 获取会话列表

#### 请求信息
- **URL**：`/Main/sessions`
- **方法**：`GET`
- **请求头**：
  - `Auth`: 认证密钥

#### 响应信息
- **成功状态码**：200 OK
- **响应体**：会话列表数组
  ```json
  [
    {
      "ID": "会话ID",
      "Title": "会话标题",
      "UpdatedAt": 时间戳,
      // 其他会话相关字段
    }
  ]
  ```
- **错误响应**：
  - 401 Unauthorized: "无效的Auth请求头"
  - 400 Bad Request: "配置未完成，请检查 config/config.txt"
  - 500 Internal Server Error: "无法获取会话列表"

### 2. 获取模型列表

#### 请求信息
- **URL**：`/Main/models`
- **方法**：`GET`
- **请求头**：
  - `Auth`: 认证密钥

#### 响应信息
- **成功状态码**：200 OK
- **响应体**：模型列表数组
  ```json
  [
    {
      "ID": "模型ID",
      "Name": "模型名称",
      "Object": "模型类型",
      "OwnedBy": "所有者",
      "Info": {
        "ID": "模型信息ID",
        "UID": "用户ID",
        "BaseModelID": "基础模型ID",
        "Name": "模型信息名称",
        "MetaData": {
          "Description": "模型描述",
          "Capabilities": {
            "Vision": true,
            "Document": false,
            // 其他能力字段
          },
          // 其他元数据字段
        }
        // 其他模型信息字段
      }
    }
  ]
  ```
- **错误响应**：
  - 401 Unauthorized: "无效的Auth请求头"
  - 500 Internal Server Error: "无法获取模型列表"

### 3. 创建新会话

#### 请求信息
- **URL**：`/Main/sessions`
- **方法**：`POST`
- **请求头**：
  - `Auth`: 认证密钥
  - `Content-Type`: application/json

#### 响应信息
- **成功状态码**：200 OK
- **响应体**：新会话详情
  ```json
  {
    "Id": "会话ID",
    "UserId": "用户ID",
    "Title": "会话标题",
    "Chat": {
      "History": {
        "Messages": {},
        "CurrentId": "",
        "CurrentResponseIds": []
      },
      "Messages": [],
      "Models": ["模型ID"]
    },
    // 其他会话相关字段
  }
  ```
- **错误响应**：
  - 401 Unauthorized: "无效的Auth请求头"
  - 400 Bad Request: "配置未完成"
  - 500 Internal Server Error: "创建会话失败"

### 4. 获取会话历史

#### 请求信息
- **URL**：`/Main/sessions/{sessionId}`
- **方法**：`GET`
- **路径参数**：
  - `sessionId`: 会话ID（GUID格式）
- **请求头**：
  - `Auth`: 认证密钥

#### 响应信息
- **成功状态码**：200 OK
- **响应体**：会话历史详情（同创建会话的响应格式）
- **错误响应**：
  - 401 Unauthorized: "无效的Auth请求头"
  - 400 Bad Request: "无效的会话ID"
  - 404 Not Found: "会话不存在或已失效"

### 5. 发送消息

#### 请求信息
- **URL**：`/Main/sessions/{sessionId}/messages`
- **方法**：`POST`
- **路径参数**：
  - `sessionId`: 会话ID
- **请求头**：
  - `Auth`: 认证密钥
  - `Content-Type`: application/json
- **请求体**：
  ```json
  {
    "Content": "消息内容"
  }
  ```

#### 响应信息
- **响应类型**：流式响应（text/plain; charset=utf-8）
- **成功状态**：持续返回消息内容片段
- **响应示例**：
  ```
  这是AI的
  回复内容
  片段...
  ```
- **错误响应**：
  - 401 Unauthorized: "无效的Auth请求头"
  - 400 Bad Request: "配置未完成" 或 "消息数据不能为空"
  - 流式响应中可能包含错误信息：`[ERROR]错误描述` 或 `[REQUEST_ERROR]错误描述`

## 部署与配置说明

1. **账户凭证配置**：
   - 运行项目后会生成`config/config.txt`文件
   - 需填入从Qwen网页版`/api/v2/chat/completions`接口获取的以下信息：
     ```
     bx-ua
     cookie
     bx-umidtoken
     ```

2. **Windows(Server)部署**：
   - 可直接构建`QwenAPIGUI`并上传到服务器
   - 启动后输入Email和Password（或使用其他登录方式）
   - 刷新WebView2页面，点击`Auth`中的`WebAPI`按钮启动服务

3. **认证密钥修改**：
   - 编辑`QwenWebAPI/Controllers/MainController.cs`文件
   - 修改`IsAuthValid`方法中的`expected`字段值

## 示例请求（HTTP）

```http
### 获取会话列表
GET /Main/sessions HTTP/1.1
Host: localhost:端口号
Auth: 0a026da3735c2b81c6f318493e3e94d43dc9da12c34360190fe5a7eb8d24d365

### 发送消息
POST /Main/sessions/会话ID/messages HTTP/1.1
Host: localhost:端口号
Auth: 0a026da3735c2b81c6f318493e3e94d43dc9da12c34360190fe5a7eb8d24d365
Content-Type: application/json

{
  "Content": "你好，请问有什么可以帮助我的？"
}
```
