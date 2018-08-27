import * as React from "react";
import { connect } from "react-redux";

import { Photograph } from "../api/photograph";
import S3Image from "../components/S3Image";
import ViewHeader from "../components/ViewHeader";
import * as redux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

export interface Props {
  getPhotograph: (id: string) => void;

  id: string;

  loading: boolean;
  error?: Error;
  photograph?: Photograph;
}

export class PhotographDetail extends React.Component<Props> {
  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotograph(this.props.id);
  }

  public render() {
    const { loading, error, photograph } = this.props;

    return (
      <div>
        <ViewHeader
          title={photograph ? photograph.Title : "Photograph Detail"}
        />

        <ul className="list-group">
          {loading && (
            <div className="alert alert-light" role="alert">
              Loading...
            </div>
          )}
          {error && (
            <div className="alert alert-danger" role="alert">
              <strong>Error: </strong>
              {error.toString()}
            </div>
          )}
          {photograph && (
            <div>
              <S3Image imageKey={photograph.Images[1].ObjectKey} />
            </div>
          )}
        </ul>
      </div>
    );
  }
}

interface RouterProps {
  match: { params: { id: string } };
}

export function mapStateToProps(
  { photographs }: RootState,
  ownProps: RouterProps
) {
  const id = ownProps.match.params.id;

  return {
    id,

    error: photographs.error,
    loading: photographs.loading,
    photograph: photographs.byId[id]
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotograph: (id: string) => dispatch(redux.getPhotograph(id))
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PhotographDetail);
