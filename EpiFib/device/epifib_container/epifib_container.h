// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#ifndef EPIFIB_CONTAINER_H
#define EPIFIB_CONTAINER_H

#ifdef __cplusplus
extern "C" {
#endif

void epifib_container_run(volatile byte* door, void (*updateLeds)(int), void (*soundAlarm)(void));

#ifdef __cplusplus
}
#endif

#endif /* EPIFIB_CONTAINER_H */
