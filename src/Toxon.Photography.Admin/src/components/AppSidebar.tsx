import * as React from "react";
import * as Icon from "react-feather";
import { NavLink } from "react-router-dom";

import "./AppSidebar.css";

export interface Props {
  currentPathName?: string;
}

const AppSidebar = ({ currentPathName }: Props) => {
  return (
    <div className="sidebar-sticky">
      <ul className="nav flex-column">
        <li className="nav-item">
          <NavLink className="nav-link" exact to="/">
            <Icon.Home size="16" /> Home
          </NavLink>
        </li>

        <li className="nav-item">
          <NavLink className="nav-link" to="/photographs">
            <Icon.Image size="16" /> Photographs
          </NavLink>
        </li>
      </ul>

      <h6 className="sidebar-heading d-flex justify-content-between align-items-center px-3 mt-4 mb-1 text-muted">
        <span>Hello USERNAME</span>
      </h6>

      <ul className="nav flex-column">
        <li className="nav-item">
          <NavLink className="nav-link" to="#">
            <Icon.Lock size="16" /> Logout
          </NavLink>
        </li>
      </ul>
    </div>
  );
};

export default AppSidebar;
