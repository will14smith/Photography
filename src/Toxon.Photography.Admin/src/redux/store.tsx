import { RouterState } from "connected-react-router";

import { State as PhotographState } from "./photograph";

export type ValueState = number;

export interface RootState {
  photographs: PhotographState;
  value: ValueState;
  router: RouterState;
}
