# 🚀 BIM Backend API Documentation

## 🔐 Authentication & User Management

### Register New User
```http
POST /api/auth/register
```
**Request Body:**
```json
{
  "login": "user1",
  "userName": "Имя",
  "userSurname": "Фамилия",
  "email": "user1@mail.com",
  "password": "пароль",
  "confirmPassword": "пароль",
  "companyName": "Компания",
  "companyPosition": "Должность"
}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "token": "jwt_token",
    "userId": 1
  }
}
```

### Login
```http
POST /api/auth/login
```
**Request Body:**
```json
{
  "login": "user1",
  "password": "пароль"
}
```
**Response:** Same as registration

### Get User Info
```http
GET /api/auth/getinfo?id=1
```
**Response:** User object with all details

## 📁 Project Management

### Get User's Projects
```http
GET /api/project?userId=1
```
**Response:** Array of project objects with access levels

### Get Project by ID
```http
GET /api/project/{id}
```
**Response:** Detailed project object

### Create Project
```http
POST /api/project
```
**Request Body:**
```json
{
  "creatorId": 1,
  "title": "Project Name"
}
```

### Update Project
```http
PUT /api/project/{id}
```
**Request Body:** Same as creation

### Delete Project
```http
DELETE /api/project/{id}
```

### List All Projects (Paginated)
```http
GET /api/project/list?skip=0&take=10
```
**Response:**
```json
{
  "total": 1,
  "projects": [...]
}
```

## 👥 Project Access Control

### Get Project Access List
```http
GET /api/project/{projectId}/access
```
**Response:** Array of access objects with user details

### Update User Access (Creator Only)
```http
POST /api/project/{projectId}/access
```
**Request Body:**
```json
{
  "userId": 2,
  "accessLevel": "Edit"
}
```

### Remove User Access (Creator Only)
```http
DELETE /api/project/{projectId}/access/{userId}
```

## 📎 File Management

### Upload Files
```http
POST /api/project/{projectId}/files
```
**FormData:** `files: [file1, file2, ...]`

### List Project Files
```http
GET /api/project/{projectId}/files
```
**Response:** Array of file objects with metadata

### Download All Files (ZIP)
```http
GET /api/project/{projectId}/files/download
```
**Response:** ZIP archive

### Download Single File
```http
GET /api/project/files/{fileId}/download
```
**Response:** File content

### Delete File
```http
DELETE /api/project/files/{fileId}
```

### Update File Content
```http
PUT /api/project/files/{fileId}
```
**FormData:** `newFile: [file]`

### Rename File
```http
PUT /api/project/files/{fileId}/rename
```
**Request Body:**
```json
{
  "newFileName": "newname.txt"
}
```

## 💬 Comments

### Add Comment
```http
POST /api/project/files/{fileId}/comments
```
**Request Body:**
```json
{
  "text": "Комментарий",
  "elementName": "element",
  "elementId": 123
}
```

### Get Comment
```http
GET /api/project/comments/{commentId}
```

### Update Comment (Author Only)
```http
PUT /api/project/comments/{commentId}
```
**Request Body:** Same as creation

### Delete Comment (Author Only)
```http
DELETE /api/project/comments/{commentId}
```

### List File Comments
```http
GET /api/project/files/{fileId}/comments
```

## 👥 User Management

### List Users (Paginated)
```http
GET /api/project/users?skip=0&take=10
```
**Response:**
```json
{
  "total": 2,
  "users": [...]
}
```

## 🔍 IFC File Collisions

### Get File Collisions
```http
GET /api/project/files/{fileId}/collisions
```
**Response:** Array of collision objects

---

## 🔒 Authentication
For protected endpoints, include the JWT token in the Authorization header:
```
Authorization: Bearer {token}
```

## 📝 Notes
- All timestamps are in UTC
- File operations maintain original file extensions
- IFC file collisions are automatically detected on upload
- Access levels: "View", "Edit", "Admin"

## 🚀 Development
- Base URL: `http://localhost:5079`
- Swagger UI available at root path
- CORS enabled for `http://localhost:3000` 