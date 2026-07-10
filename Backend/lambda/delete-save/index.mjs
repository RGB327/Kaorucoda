import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { DynamoDBDocumentClient, DeleteCommand } from "@aws-sdk/lib-dynamodb";

const client = DynamoDBDocumentClient.from(new DynamoDBClient({}));
const TABLE_NAME = process.env.TABLE_NAME;

// DELETE /save — userId는 검증된 JWT claim에서만 가져온다(다른 계정 세이브를 지울 수 없음).
export const handler = async (event) => {
  const userId = event.requestContext?.authorizer?.jwt?.claims?.sub;
  if (!userId) {
    return { statusCode: 401, body: JSON.stringify({ message: "Unauthorized" }) };
  }

  await client.send(new DeleteCommand({
    TableName: TABLE_NAME,
    Key: { userId },
  }));

  return { statusCode: 200, body: JSON.stringify({ ok: true }) };
};
