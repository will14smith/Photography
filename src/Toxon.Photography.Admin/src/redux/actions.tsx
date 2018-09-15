import { RouterAction } from "connected-react-router";

import { LayoutAction } from "./layout";
import { PhotographAction } from "./photograph";

export type RootAction = LayoutAction | PhotographAction | RouterAction;
