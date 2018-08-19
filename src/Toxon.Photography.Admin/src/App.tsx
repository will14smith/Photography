import * as React from "react";

import { ConnectedRouter } from "connected-react-router";
import { History } from "history";

import AppHeader from "./components/AppHeader";
import AppSidebar from "./components/AppSidebar";
import routes from "./routes";

import "./App.css";

interface AppProps {
  history: History;
}

const App = ({ history }: AppProps) => {
  return (
    <ConnectedRouter history={history}>
      <div>
        <AppHeader />

        <div className="container-fluid">
          <div className="row">
            <nav className="col-md-2 d-none d-md-block bg-light sidebar">
              <AppSidebar />
            </nav>

            <main role="main" className="col-md-9 ml-sm-auto col-lg-10 px-4">
              {routes}
            </main>
          </div>
        </div>
      </div>
    </ConnectedRouter>
  );
};

export default App;
