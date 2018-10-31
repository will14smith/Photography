import { Dispatch } from "redux";

import * as layoutApi from "../api/layout";
import {
  getPhotographs,
  GetPhotographsSucceeded,
  PhotographAction
} from "./photograph";
import { RootState } from "./store";
import { Action } from "./types";

export interface LayoutItem {
  id: string;

  width: number | null;
  height: number | null;
}

// Actions

export const RESET_LAYOUT = "RESET_LAYOUT";
export const SET_LAYOUT = "SET_LAYOUT";
export type RESET_LAYOUT = typeof RESET_LAYOUT;
export type SET_LAYOUT = typeof SET_LAYOUT;

export interface ResetLayout {
  type: RESET_LAYOUT;
}
export interface SetLayout {
  type: SET_LAYOUT;
  layout: LayoutItem[];
}

export function resetLayout(): ResetLayout {
  return { type: RESET_LAYOUT };
}
export function saveLayout(model: LayoutItem[]): Action {
  return async (
    dispatch: Dispatch<LayoutAction | PhotographAction>,
    getState: () => RootState
  ) => {
    const edit: layoutApi.LayoutEdit = {};

    model.forEach((item, idx) => {
      edit[item.id] = { Order: idx, Width: item.width, Height: item.height };
    });

    await layoutApi.edit(edit);

    return getPhotographs()(dispatch, getState, null);
  };
}
export function setLayout(layout: LayoutItem[]): SetLayout {
  return { type: SET_LAYOUT, layout };
}

export type LayoutAction = ResetLayout | SetLayout;

// State

export interface State {
  initialLayout: LayoutItem[];

  currentLayout: LayoutItem[];
}

const initialState: State = {
  initialLayout: [],

  currentLayout: []
};

// Reducer
export function reducer(
  state: State = initialState,
  action: LayoutAction | GetPhotographsSucceeded
): State {
  switch (action.type) {
    case "GET_PHOTOGRAPHS_SUCCEEDED":
      const layout = action.photographs
        .filter(x => x.Layout !== undefined)
        .sort(
          (a, b) =>
            (a.Layout || { Order: 0 }).Order - (b.Layout || { Order: 0 }).Order
        )
        .map(x => ({
          id: x.Id,

          height: x.Layout ? x.Layout.Height : null,
          width: x.Layout ? x.Layout.Width : null
        }));

      return { initialLayout: layout, currentLayout: layout };

    case SET_LAYOUT:
      return { ...state, currentLayout: action.layout };

    case RESET_LAYOUT:
      return { ...state, currentLayout: state.initialLayout };
  }
  return state;
}
