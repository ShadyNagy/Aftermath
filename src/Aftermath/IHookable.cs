using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aftermath;

/// <summary>
/// Marker interface that indicates a class or interface should be processed for hooks.
/// </summary>
public interface IHookable
{
	// This is a marker interface with no members.
	// Classes or interfaces that implement this interface will be automatically registered for hook processing.
}