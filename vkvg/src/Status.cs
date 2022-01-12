// Copyright (c) 2021-2022  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;

namespace vkvg {
	public enum Status {
		VKVG_STATUS_SUCCESS = 0,			/*!< no error occurred.*/
		VKVG_STATUS_NO_MEMORY,				/*!< out of memory*/
		VKVG_STATUS_INVALID_RESTORE,		/*!< call to #vkvg_restore without matching call to #vkvg_save*/
		VKVG_STATUS_NO_CURRENT_POINT,		/*!< path command expecting a current point to be defined failed*/
		VKVG_STATUS_INVALID_MATRIX,			/*!< invalid matrix (not invertible)*/
		VKVG_STATUS_INVALID_STATUS,			/*!< */
		VKVG_STATUS_INVALID_INDEX,			/*!< */
		VKVG_STATUS_NULL_POINTER,			/*!< NULL pointer*/
		VKVG_STATUS_INVALID_STRING,			/*!< */
		VKVG_STATUS_INVALID_PATH_DATA,		/*!< */
		VKVG_STATUS_READ_ERROR,				/*!< */
		VKVG_STATUS_WRITE_ERROR,			/*!< */
		VKVG_STATUS_SURFACE_FINISHED,		/*!< */
		VKVG_STATUS_SURFACE_TYPE_MISMATCH,	/*!< */
		VKVG_STATUS_PATTERN_TYPE_MISMATCH,	/*!< */
		VKVG_STATUS_PATTERN_INVALID_GRADIENT,/*!< occurs when stops count is zero */
		VKVG_STATUS_INVALID_CONTENT,		/*!< */
		VKVG_STATUS_INVALID_FORMAT,			/*!< */
		VKVG_STATUS_INVALID_VISUAL,			/*!< */
		VKVG_STATUS_FILE_NOT_FOUND,			/*!< */
		VKVG_STATUS_INVALID_DASH,			/*!< invalid value for a dash setting */
		VKVG_STATUS_INVALID_RECT,			/*!< rectangle with height or width equal to 0. */
		VKVG_STATUS_TIMEOUT,				/*!< waiting for a vulkan operation to finish resulted in a fence timeout (5 seconds)*/
	};
}
