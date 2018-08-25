import { Auth } from "aws-amplify";

import { ENDPOINT } from "./config";

export default async function fetchWithAuthentication(
  url: string,
  init: RequestInit = {}
): Promise<Response> {
  const session = await Auth.currentSession();
  const token = session.idToken.jwtToken;

  return fetch(ENDPOINT + url, {
    ...init,
    headers: { ...init.headers, Authorization: "Bearer " + token }
  });
}
