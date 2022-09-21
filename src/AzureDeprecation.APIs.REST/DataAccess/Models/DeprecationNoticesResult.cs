﻿namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class DeprecationNoticesResult
{
    public IReadOnlyList<NoticeEntity> Deprecations { get; set; } = new List<NoticeEntity>();
}