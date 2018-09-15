import { RouterState } from "connected-react-router";

import { State as LayoutState } from "./layout";
import { State as PhotographState } from "./photograph";

export interface RootState {
  layout: LayoutState;
  photographs: PhotographState;

  router: RouterState;
}
