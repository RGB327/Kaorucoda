# AWS 백엔드 설정

로그인(Cognito)과 세이브 데이터(API Gateway + Lambda + DynamoDB) 인프라 설정 기록.
Lambda 소스코드는 이 폴더(`Backend/lambda/`) 안에 있음.

## 1. Cognito User Pool (로그인)

| 항목 | 값 |
|---|---|
| Region | `us-east-1` |
| User Pool ID | `us-east-1_hhrZ0sosu` |
| 로그인 식별자 | User name만 (Email/Phone 별도 없음) |
| 필수 속성 | `email` (실제로는 유저한테 안 받고, Unity `AuthManager`가 `{아이디}@noemail.local`로 자동 생성해서 채움) |
| MFA | Off |
| 자체 회원가입 | 켬 |
| 비밀번호 정책 | AWS 기본값 — 8자 이상, 대문자/소문자/숫자/특수문자 각 1개 이상 |

### Pre sign-up Lambda 트리거
- 함수: 회원가입 즉시 이메일 인증 없이 자동 확인(Confirmed) 처리
- 코드: [`lambda/pre-signup/index.mjs`](lambda/pre-signup/index.mjs)
- 연결 위치: User Pool → Extensions → Lambda triggers → Pre sign-up

### App Client
| 항목 | 값 |
|---|---|
| Client ID | `5n9ih7vies3n6sfqdi4shs8enb` |
| Client secret | 없음 (Public client — Unity 빌드에 시크릿 못 넣으므로) |
| Authentication flows | `ALLOW_USER_PASSWORD_AUTH` (수동으로 켬), `ALLOW_REFRESH_TOKEN_AUTH` (기본값) |
| Refresh token 만료 | 3650일 (최대) — 로그아웃 전까지 자동 로그인 유지 목적 |

Unity에서는 SDK 없이 Cognito의 공개 HTTPS JSON API를 `UnityWebRequest`로 직접 호출함
(`Assets/Script/auth/CognitoAuthClient.cs`). SigV4 서명 불필요.

## 2. 세이브 데이터 (DynamoDB + Lambda + API Gateway)

### DynamoDB 테이블: `GameSaveData`
- 파티션 키: `userId` (문자열, Cognito `sub` claim)
- 아이템 형태: `{ userId, saveBlob(문자열, 게임이 정의한 JSON), schemaVersion, updatedAt }`

### Lambda 함수
| 함수 | 라우트 | 코드 | 상태 |
|---|---|---|---|
| `GetSaveData` | `GET /save` | [`lambda/get-save/index.mjs`](lambda/get-save/index.mjs) | 배포 완료 |
| `PutSaveData` | `PUT /save` | [`lambda/put-save/index.mjs`](lambda/put-save/index.mjs) | 배포 완료 |
| `DeleteSaveData` | `DELETE /save` | [`lambda/delete-save/index.mjs`](lambda/delete-save/index.mjs) | **코드만 있음, AWS 배포 안 함** |

각 Lambda 공통:
- 환경 변수: `TABLE_NAME=GameSaveData`
- 실행 역할에 해당 테이블 대상 DynamoDB 권한 필요
- `userId`는 항상 요청 바디가 아니라 **검증된 JWT claim(`event.requestContext.authorizer.jwt.claims.sub`)**에서만 가져옴 — 클라이언트가 다른 계정 데이터를 건드릴 수 없게 하는 핵심 장치

### API Gateway (HTTP API)
| 항목 | 값 |
|---|---|
| Invoke URL | `https://cpupvyunhl.execute-api.us-east-1.amazonaws.com` |
| Authorizer | JWT, Issuer = `https://cognito-idp.us-east-1.amazonaws.com/us-east-1_hhrZ0sosu`, Audience = App Client ID |
| 라우트 | `GET /save`, `PUT /save` (Authorization 필수) — `DELETE /save`는 아직 라우트 자체가 없음 |

## 3. Unity 쪽 설정 연결

위 `Region`/`Client ID`/`Invoke URL` 값은 코드에 하드코딩하지 않고
`Assets/StreamingAssets/auth-config.json`에서 읽음 (`.gitignore`로 실제 파일은 제외,
`auth-config.example.json`이 템플릿으로 커밋돼 있음). 새로 프로젝트 받은 사람은
example 파일을 복사해서 위 표의 값을 채우면 됨.

## 4. 남은 작업

- [ ] `DeleteSaveData` Lambda AWS 콘솔에 실제 생성/배포 (코드는 준비됨)
- [ ] API Gateway에 `DELETE /save` 라우트 추가 + 기존 JWT Authorizer 연결
- [ ] Unity 씬에 `ContinuePopup`, `ErrorPopup` 오브젝트 생성 및 필드 연결
- [ ] Build Settings(Scenes In Build)에 `SampleScene`, `SampleScene2` 추가
