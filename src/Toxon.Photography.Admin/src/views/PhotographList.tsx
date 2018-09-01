import * as React from "react";
import * as Icon from "react-feather";
import { connect } from "react-redux";
import { Link } from "react-router-dom";

import { Photograph } from "../api/photograph";
import PhotographThumbnail from "../components/PhotographThumbnail";
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

        <ul className="list-group">
          {this.props.loading && (
            <li className="list-group-item list-group-item-light">
              Loading...
            </li>
          )}
          {this.props.error && (
            <li className="list-group-item list-group-item-danger">
              <strong>Error: </strong>
              {this.props.error.toString()}
            </li>
          )}
          {!this.props.loading &&
            !this.props.error &&
            (this.props.photographs.length !== 0 ? (
              this.props.photographs.map(photograph => (
                <li key={photograph.Id} className="list-group-item">
                  <Link to={`/photographs/${photograph.Id}`}>
                    <div className="row align-items-center">
                      <div className="col-sm-auto">
                        <PhotographThumbnail
                          photograph={photograph}
                          width="50px"
                        />
                      </div>
                      <div className="col-sm">
                        <h3>{photograph.Title}</h3>
                      </div>
                    </div>
                  </Link>
                </li>
              ))
            ) : (
              <li className="list-group-item list-group-item-light">
                No photographs, add one!
              </li>
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
