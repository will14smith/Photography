import { Dispatch } from "redux";

import { get, getAll, Photograph } from "../api/photograph";
import { Action } from "./types";

// Actions
export const GET_PHOTOGRAPH_STARTED = "GET_PHOTOGRAPH_STARTED";
export const GET_PHOTOGRAPH_SUCCESS = "GET_PHOTOGRAPH_SUCCEEDED";
export const GET_PHOTOGRAPH_FAILED = "GET_PHOTOGRAPH_FAILED";
export type GET_PHOTOGRAPH_STARTED = typeof GET_PHOTOGRAPH_STARTED;
export type GET_PHOTOGRAPH_SUCCEEDED = typeof GET_PHOTOGRAPH_SUCCESS;
export type GET_PHOTOGRAPH_FAILED = typeof GET_PHOTOGRAPH_FAILED;

export const GET_PHOTOGRAPHS_STARTED = "GET_PHOTOGRAPHS_STARTED";
export const GET_PHOTOGRAPHS_SUCCESS = "GET_PHOTOGRAPHS_SUCCEEDED";
export const GET_PHOTOGRAPHS_FAILED = "GET_PHOTOGRAPHS_FAILED";
export type GET_PHOTOGRAPHS_STARTED = typeof GET_PHOTOGRAPHS_STARTED;
export type GET_PHOTOGRAPHS_SUCCEEDED = typeof GET_PHOTOGRAPHS_SUCCESS;
export type GET_PHOTOGRAPHS_FAILED = typeof GET_PHOTOGRAPHS_FAILED;

export interface GetPhotographStarted {
  type: GET_PHOTOGRAPH_STARTED;
}
export interface GetPhotographSucceeded {
  type: GET_PHOTOGRAPH_SUCCEEDED;
  photograph: Photograph;
}
export interface GetPhotographFailed {
  type: GET_PHOTOGRAPH_FAILED;
  error: Error;
}

export interface GetPhotographsStarted {
  type: GET_PHOTOGRAPHS_STARTED;
}
export interface GetPhotographsSucceeded {
  type: GET_PHOTOGRAPHS_SUCCEEDED;
  photographs: Photograph[];
}
export interface GetPhotographsFailed {
  type: GET_PHOTOGRAPHS_FAILED;
  error: Error;
}

export type PhotographAction =
  | GetPhotographStarted
  | GetPhotographSucceeded
  | GetPhotographFailed
  | GetPhotographsStarted
  | GetPhotographsSucceeded
  | GetPhotographsFailed;

export function getPhotographStarted(): GetPhotographStarted {
  return { type: GET_PHOTOGRAPH_STARTED };
}
export function getPhotographSucceeded(
  photograph: Photograph
): GetPhotographSucceeded {
  return { type: GET_PHOTOGRAPH_SUCCESS, photograph };
}
export function getPhotographFailed(error: Error): GetPhotographFailed {
  return { type: GET_PHOTOGRAPH_FAILED, error };
}

export function getPhotographsStarted(): GetPhotographsStarted {
  return { type: GET_PHOTOGRAPHS_STARTED };
}
export function getPhotographsSucceeded(
  photographs: Photograph[]
): GetPhotographsSucceeded {
  return { type: GET_PHOTOGRAPHS_SUCCESS, photographs };
}
export function getPhotographsFailed(error: Error): GetPhotographsFailed {
  return { type: GET_PHOTOGRAPHS_FAILED, error };
}

export function getPhotograph(id: string): Action {
  return async (dispatch: Dispatch<PhotographAction>) => {
    dispatch(getPhotographStarted());
    try {
      const photograph = await get(id);
      dispatch(getPhotographSucceeded(photograph));
    } catch (e) {
      dispatch(getPhotographFailed(e));
    }
  };
}

export function getPhotographs(): Action {
  return async (dispatch: Dispatch<PhotographAction>) => {
    dispatch(getPhotographsStarted());
    try {
      const photographs = await getAll();
      dispatch(getPhotographsSucceeded(photographs));
    } catch (e) {
      dispatch(getPhotographsFailed(e));
    }
  };
}

// state
export interface State {
  loading: boolean;
  error?: Error;

  byId: { [id: string]: Photograph };
  ids: string[];
}

const initialState: State = {
  loading: false,

  byId: {},
  ids: []
};

// reducer
export function reducer(
  state: State = initialState,
  action: PhotographAction
): State {
  switch (action.type) {
    case GET_PHOTOGRAPH_STARTED:
      return { ...state, loading: true, error: undefined };
    case GET_PHOTOGRAPH_SUCCESS: {
      const ids: string[] = [];
      const byId: { [id: string]: Photograph } = {};

      ids.push(action.photograph.Id);
      byId[action.photograph.Id] = action.photograph;

      return { ...state, loading: false, error: undefined, byId, ids };
    }
    case GET_PHOTOGRAPH_FAILED:
      return { ...state, loading: false, error: action.error };

    case GET_PHOTOGRAPHS_STARTED:
      return { ...state, loading: true, error: undefined };
    case GET_PHOTOGRAPHS_SUCCESS: {
      const ids: string[] = [];
      const byId: { [id: string]: Photograph } = {};

      action.photographs.forEach(photograph => {
        ids.push(photograph.Id);
        byId[photograph.Id] = photograph;
      });

      return { ...state, loading: false, error: undefined, byId, ids };
    }
    case GET_PHOTOGRAPHS_FAILED:
      return { ...state, loading: false, error: action.error };

    default:
      return state;
  }
}
