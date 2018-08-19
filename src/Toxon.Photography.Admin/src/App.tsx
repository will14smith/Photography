import * as React from "react";

import { ConnectedRouter } from "connected-react-router";
import { History } from "history";
import { Link } from "react-router-dom";

import routes from "./routes";

interface AppProps {
  history: History;
}

const App = ({ history }: AppProps) => {
  return (
    <ConnectedRouter history={history}>
      <div>
        <ul>
          <li>
            <Link to="/">Home</Link>
          </li>
          <li>
            <Link to="/value">Value</Link>
          </li>
        </ul>
        {routes}
      </div>
    </ConnectedRouter>
  );
};

export default App;
