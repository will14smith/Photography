import { RouterState } from "connected-react-router";

export type ValueState = number;

export interface RootState {
  value: ValueState;
  router: RouterState;
}
