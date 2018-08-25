import * as React from "react";
import * as ReactDOM from "react-dom";

import { connectRouter, routerMiddleware } from "connected-react-router";
import { createBrowserHistory } from "history";
import { AppContainer } from "react-hot-loader";
import { Provider } from "react-redux";
import { applyMiddleware, compose, createStore } from "redux";
import thunk from "redux-thunk";

import Amplify from "aws-amplify";
import { Authenticator, Greetings } from "aws-amplify-react";
import amplifyConfig from "./amplifyConfig";

import App from "./App";
import "./index.css";
import rootReducer from "./redux/reducer";
import registerServiceWorker from "./registerServiceWorker";

Amplify.configure(amplifyConfig);

const history = createBrowserHistory();

const composeEnhancer: typeof compose =
  (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;
const store = createStore(
  connectRouter(history)(rootReducer),
  composeEnhancer(applyMiddleware(routerMiddleware(history), thunk))
);

const render = () => {
  ReactDOM.render(
    <AppContainer>
      <Provider store={store}>
        <Authenticator hide={[Greetings]}>
          <App history={history} />
        </Authenticator>
      </Provider>
    </AppContainer>,
    document.getElementById("root")
  );
};

render();

// Hot reloading
if (module.hot) {
  // Reload components
  module.hot.accept("./App", () => {
    render();
  });

  // Reload reducers
  module.hot.accept("./redux/reducer", () => {
    store.replaceReducer(connectRouter(history)(rootReducer));
  });
}

registerServiceWorker();
