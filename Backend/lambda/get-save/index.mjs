import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import { DynamoDBDocumentClient, GetCommand } from "@aws-sdk/lib-dynamodb";

const client = DynamoDBDocumentClient.from(new DynamoDBClient({}));
const TABLE_NAME = process.env.TABLE_NAME;

// GET /save — API Gateway JWT Authorizer가 이미 토큰을 검증했으므로
// claims.sub 를 그대로 신뢰해서 그 유저의 세이브만 돌려준다.
export const handler = async (event) => {
  const userId = event.requestContext?.authorizer?.jwt?.claims?.sub;
  if (!userId) {
    return { statusCode: 401, body: JSON.stringify({ message: "Unauthorized" }) };
  }

  const result = await client.send(new GetCommand({
    TableName: TABLE_NAME,
    Key: { userId },
  }));

  if (!result.Item) {
    return { statusCode: 404, body: JSON.stringify({ message: "no save yet" }) };
  }

  return {
    statusCode: 200,
    body: JSON.stringify({
      saveBlob: result.Item.saveBlob,
      schemaVersion: result.Item.schemaVersion,
      updatedAt: result.Item.updatedAt,
    }),
  };
};
