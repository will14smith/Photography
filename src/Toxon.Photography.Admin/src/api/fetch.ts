import { Auth } from "aws-amplify";

import { ENDPOINT } from "./config";

async function requestWithAuthentication(
  init: RequestInit
): Promise<RequestInit> {
  const session = await Auth.currentSession();
  const token = session.idToken.jwtToken;

  return {
    ...init,
    headers: { ...init.headers, Authorization: "Bearer " + token }
  };
}

export async function fetchWithAuthentication(
  url: string,
  init: RequestInit = {}
): Promise<Response> {
  const authenticatedInit = await requestWithAuthentication(init);

  return fetch(ENDPOINT + url, authenticatedInit);
}
