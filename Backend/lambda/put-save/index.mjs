import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { DynamoDBDocumentClient, PutCommand } from "@aws-sdk/lib-dynamodb";

const client = DynamoDBDocumentClient.from(new DynamoDBClient({}));
const TABLE_NAME = process.env.TABLE_NAME;

// PUT /save — userId는 요청 바디가 아니라 검증된 JWT claim에서 가져온다.
// 클라이언트가 무엇을 보내든 자기 자신의 row만 덮어쓸 수 있다 (조작 방지 핵심).
export const handler = async (event) => {
  const userId = event.requestContext?.authorizer?.jwt?.claims?.sub;
  if (!userId) {
    return { statusCode: 401, body: JSON.stringify({ message: "Unauthorized" }) };
  }

  let body;
  try {
    body = JSON.parse(event.body || "{}");
  } catch {
    return { statusCode: 400, body: JSON.stringify({ message: "invalid JSON body" }) };
  }

  if (typeof body.saveBlob !== "string") {
    return { statusCode: 400, body: JSON.stringify({ message: "saveBlob (string) is required" }) };
  }

  await client.send(new PutCommand({
    TableName: TABLE_NAME,
    Item: {
      userId,
      saveBlob: body.saveBlob,
      schemaVersion: body.schemaVersion ?? 1,
      updatedAt: new Date().toISOString(),
    },
  }));

  return { statusCode: 200, body: JSON.stringify({ ok: true }) };
};
