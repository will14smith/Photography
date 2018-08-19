import * as React from "react";
import * as ReactDOM from "react-dom";

import { Provider } from "react-redux";
import { applyMiddleware, createStore } from "redux";
import thunk from "redux-thunk";

import Value from "./containers/Value";
import reducer from "./redux/reducer";
import registerServiceWorker from "./registerServiceWorker";

import "./index.css";

const store = createStore(reducer, applyMiddleware(thunk));

ReactDOM.render(
  <Provider store={store}>
    <Value />
  </Provider>,
  document.getElementById("root") as HTMLElement
);
registerServiceWorker();
