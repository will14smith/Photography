import { combineReducers } from "redux";

import { reducer as layoutReducer } from "./layout";
import { reducer as photographReducer } from "./photograph";

const rootReducer = combineReducers({
  layout: layoutReducer,
  photographs: photographReducer
});

export default rootReducer;
