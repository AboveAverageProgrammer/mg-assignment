namespace ProductManagerApi.Models;

public enum CreateResultCode { Created, DuplicateName }
public enum UpdateResultCode { Ok, NotFound, PreconditionFailed, DuplicateName }
public enum DeleteResult { Ok, NotFound, PreconditionFailed }


public record CreateResult(CreateResultCode Code, int? Id);
public record UpdateResult(UpdateResultCode Code, uint? NewVersion);
