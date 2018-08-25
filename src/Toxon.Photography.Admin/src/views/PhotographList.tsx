import * as React from "react";
import * as Icon from "react-feather";
import { connect } from "react-redux";
import { Link } from "react-router-dom";

import { Photograph } from "../api/photograph";
import ViewHeader from "../components/ViewHeader";
import * as redux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

export interface Props {
  getPhotographs: () => void;

  loading: boolean;
  error?: Error;
  photographs: Photograph[];
}

export class PhotographList extends React.Component<Props> {
  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotographs();
  }

  public render() {
    return (
      <div>
        <ViewHeader title="Photographs">
          <Link
            to="/photographs/create"
            className="btn btn-sm btn-outline-secondary"
          >
            <Icon.Plus />
          </Link>
        </ViewHeader>

        <ul>
          {this.props.loading && <li>Loading...</li>}
          {this.props.error && (
            <li>
              <strong>Error: </strong>
              {this.props.error.toString()}
            </li>
          )}
          {!this.props.loading &&
            (this.props.photographs.length !== 0 ? (
              this.props.photographs.map(photograph => (
                <li key={photograph.Id}>
                  {photograph.Id} - {photograph.Title}
                </li>
              ))
            ) : (
              <li>No photographs, add one!</li>
            ))}
        </ul>
      </div>
    );
  }
}

export function mapStateToProps({ photographs }: RootState) {
  const all = photographs.ids.map(id => photographs.byId[id]);

  return {
    error: photographs.error,
    loading: photographs.loading,
    photographs: all
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotographs: () => dispatch(redux.getPhotographs())
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PhotographList);
