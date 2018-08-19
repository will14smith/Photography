import * as React from "react";
import * as Icon from "react-feather";

const Home = () => (
  <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
    <h1 className="h2">Hello World!</h1>
    <div className="btn-toolbar mb-2 mb-md-0">
      <div className="btn-group mr-2">
        <button className="btn btn-sm btn-outline-secondary">
          <Icon.Plus />
        </button>
      </div>
    </div>
  </div>
);
export default Home;
