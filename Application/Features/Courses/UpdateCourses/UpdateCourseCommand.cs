using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Courses.UpdateCourse;

using Application.Abstractions.Messaging;
using Domain.Common;
public sealed record UpdateCourseCommand(
Guid Id,
string Title,
string Code,
int Credits
) : ICommand<Result<UpdateCourseResponse>>;

public sealed record UpdateCourseResponse(
Guid Id,
string Title,
string Code,
int Credits);
