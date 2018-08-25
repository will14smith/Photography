import * as React from "react";
import { connect } from "react-redux";

import { push } from "connected-react-router";

import {
  create,
  Photograph,
  PhotographCreate as PhotographCreateModel
} from "../api/photograph";
import ViewHeader from "../components/ViewHeader";
import { Dispatch } from "../redux/types";

export interface Props {
  createPhotograph: (model: PhotographCreateModel) => Promise<Photograph>;
}

interface State {
  title: string;
}

export class PhotographCreate extends React.Component<Props, State> {
  public state = { title: "" };

  constructor(props: Props) {
    super(props);
  }

  public render() {
    return (
      <div>
        <ViewHeader title="Create Photograph" />

        <form onSubmit={this.submit}>
          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input
              id="title"
              placeholder="Enter photograph title"
              required
              value={this.state.title}
              onChange={this.titleChanged}
              type="text"
              className="form-control"
            />
          </div>

          <button type="submit" className="btn btn-primary">
            Create
          </button>
        </form>
      </div>
    );
  }

  private titleChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ title: e.target.value });

  private submit = (e: React.SyntheticEvent) => {
    e.preventDefault();
    e.stopPropagation();

    this.props.createPhotograph({ Title: this.state.title });
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    createPhotograph: async (model: PhotographCreateModel) => {
      const response = await create(model);

      // TODO redirect to photograph details
      dispatch(push("/photographs"));

      return response;
    }
  };
}

export default connect(
  null,
  mapDispatchToProps
)(PhotographCreate);
