<template>
  <div class="p-3 p-md-4 text-start h-100 d-flex flex-column overflow-auto">
    <!-- Main 8:2 Split Row -->
    <div class="row g-3 flex-grow-1">
      
      <!-- ==================== LEFT COLUMN: MAIN FILE EXPLORER (8 Parts / ~75-80%) ==================== -->
      <div class="col-12 col-xl-9 col-lg-8 d-flex flex-column gap-3">
        
        <!-- Hidden file input for quick upload -->
        <input 
          ref="fileInputRef" 
          type="file" 
          class="d-none" 
          @change="handleFileInputChange"
        />

        <!-- Upload Progress Banner (when active) -->
        <div v-if="isUploading" class="card border-0 shadow-sm p-3 rounded-3 bg-body border-start border-4 border-primary">
          <div class="d-flex align-items-center justify-content-between mb-2">
            <span class="small fw-semibold text-body text-truncate me-2">
              <i class="bi bi-arrow-repeat spin me-1 text-primary"></i>
              {{ uploadingFileName }}
            </span>
            <span class="small text-muted font-monospace">{{ uploadProgress }}%</span>
          </div>
          <div class="progress" style="height: 6px;">
            <div 
              class="progress-bar progress-bar-striped progress-bar-animated bg-primary" 
              role="progressbar" 
              :style="{ width: `${uploadProgress}%` }" 
              :aria-valuenow="uploadProgress" 
              aria-valuemin="0" 
              aria-valuemax="100"
            ></div>
          </div>
        </div>

        <!-- Main Explorer Card -->
        <div class="card border-0 shadow-sm p-4 rounded-3 bg-body flex-grow-1 d-flex flex-column">
          
          <!-- Warning if Google Drive unconfigured on backend -->
          <div v-if="driveConfigWarning" class="alert alert-warning border-warning-subtle d-flex align-items-center gap-2 mb-3 rounded-3 py-2 px-3 small">
            <i class="bi bi-exclamation-triangle-fill fs-5 text-warning"></i>
            <div>
              <strong>{{ $t('files.SCR0461') }}</strong> {{ $t('files.SCR0462') }}
            </div>
          </div>

          <!-- Backlog Style Breadcrumbs & Action Toolbar (Unified with Same Height Controls) -->
          <div class="d-flex align-items-center justify-content-between flex-wrap gap-3 pb-3 mb-3 border-bottom">
            <!-- Breadcrumbs Navigation -->
            <nav :aria-label="$t('common.SCR0026')" class="d-flex align-items-center flex-wrap">
              <ol class="breadcrumb mb-0 align-items-center fs-6">
                <li class="breadcrumb-item">
                  <a 
                    href="javascript:void(0)" 
                    class="text-decoration-none d-inline-flex align-items-center gap-1.5 fw-semibold"
                    :class="!currentFolderId ? 'text-primary' : 'text-secondary'"
                    @click="navigateToFolder(null)"
                  >
                    <i class="bi bi-hdd-network-fill text-primary"></i>
                    <span>{{ $t('files.SCR0405') }}</span>
                  </a>
                </li>
                <li 
                  v-for="(crumb, idx) in breadcrumbs" 
                  :key="crumb.Id" 
                  class="breadcrumb-item"
                  :class="{ active: idx === breadcrumbs.length - 1 }"
                >
                  <a 
                    v-if="idx < breadcrumbs.length - 1" 
                    href="javascript:void(0)" 
                    class="text-decoration-none text-secondary fw-medium"
                    @click="navigateToFolder(crumb.Id)"
                  >
                    {{ crumb.Name }}
                  </a>
                  <span v-else class="text-body fw-bold">
                    {{ crumb.Name }}
                  </span>
                </li>
              </ol>
            </nav>

            <!-- Search, Action Buttons & View Mode (Uniform 38px Height) -->
            <div class="d-flex align-items-center gap-2 flex-wrap ms-auto">
              <!-- Search box -->
              <div class="position-relative" style="width: 220px; max-width: 100%;">
                <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-muted" style="pointer-events: none;"></i>
                <input 
                  v-model="searchQuery" 
                  type="text" 
                  class="form-control toolbar-control ps-5 rounded-3 border" 
                  :placeholder="$t('files.SCR0410')"
                />
              </div>

              <!-- Upload File Button -->
              <button 
                type="button" 
                class="btn btn-primary toolbar-control rounded-3 px-3 fw-semibold d-inline-flex align-items-center justify-content-center gap-2 shadow-sm"
                @click="triggerFileInput"
              >
                <i class="bi bi-cloud-arrow-up-fill"></i>
                <span>{{ $t('files.SCR0403') }}</span>
              </button>

              <!-- Create Folder Button -->
              <button 
                type="button" 
                class="btn btn-light bg-body-secondary toolbar-control border rounded-3 px-3 fw-medium d-inline-flex align-items-center justify-content-center gap-2"
                @click="openCreateFolderModal"
              >
                <i class="bi bi-folder-plus text-warning-emphasis"></i>
                <span>{{ $t('files.SCR0404') }}</span>
              </button>

              <!-- View Mode Toggle -->
              <div class="btn-group toolbar-control toolbar-btn-group border rounded-3 p-0.5 bg-body-secondary" role="group">
                <button 
                  type="button" 
                  class="btn rounded-2 px-2.5 border-0 d-inline-flex align-items-center justify-content-center"
                  :class="viewMode === 'table' ? 'btn-white bg-body text-primary shadow-sm fw-bold' : 'btn-light text-muted'"
                  @click="viewMode = 'table'"
                  :title="$t('files.SCR0463')"
                >
                  <i class="bi bi-list-ul"></i>
                </button>
                <button 
                  type="button" 
                  class="btn rounded-2 px-2.5 border-0 d-inline-flex align-items-center justify-content-center"
                  :class="viewMode === 'grid' ? 'btn-white bg-body text-primary shadow-sm fw-bold' : 'btn-light text-muted'"
                  @click="viewMode = 'grid'"
                  :title="$t('files.SCR0464')"
                >
                  <i class="bi bi-grid-3x3-gap-fill"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- BATCH ACTION BAR (Backlog Checked Items Toolbar) -->
          <div 
            v-if="selectedCount > 0" 
            class="d-flex align-items-center justify-content-between flex-wrap gap-2 px-3 py-2 bg-primary-subtle border border-primary-subtle rounded-3 mb-3 transition-all shadow-sm"
          >
            <div class="d-flex align-items-center gap-2">
              <div class="form-check mb-0 d-flex align-items-center gap-2">
                <input 
                  id="batchSelectAll"
                  class="form-check-input custom-file-checkbox my-0" 
                  type="checkbox" 
                  :checked="isAllSelected"
                  :indeterminate="isIndeterminate"
                  @change="toggleSelectAll"
                />
                <label for="batchSelectAll" class="form-check-label small fw-semibold text-body cursor-pointer">
                  {{ $t('files.SCR0465', { count: selectedCount }) }}
                </label>
              </div>
            </div>

            <div class="d-flex align-items-center gap-2 flex-wrap">
              <!-- Download Button (Single File -> direct, >= 2 Files/Folder -> .zip) -->
              <button 
                type="button" 
                class="btn btn-sm btn-primary rounded-2 px-3 py-1.5 fw-semibold d-inline-flex align-items-center gap-2 shadow-sm"
                @click="handleBatchDownload"
                :disabled="isBatchDownloading"
              >
                <span v-if="isBatchDownloading" class="spinner-border spinner-border-sm"></span>
                <i v-else :class="isZipBatch ? 'bi bi-file-earmark-zip-fill' : 'bi bi-download'"></i>
                <span>
                  {{ isZipBatch ? $t('files.SCR0466', { count: selectedCount }) : $t('files.SCR0467') }}
                </span>
              </button>

              <!-- Delete Batch Button (If user has Manager/Admin permission) -->
              <button 
                v-if="canDeleteFile"
                type="button" 
                class="btn btn-sm btn-outline-danger bg-body border-danger-subtle rounded-2 px-3 py-1.5 fw-medium d-inline-flex align-items-center gap-2"
                @click="handleBatchDelete"
                :disabled="isBatchDeleting"
              >
                <span v-if="isBatchDeleting" class="spinner-border spinner-border-sm"></span>
                <i v-else class="bi bi-trash3"></i>
                <span>{{ $t('files.SCR0468', { count: selectedCount }) }}</span>
              </button>

              <!-- Deselect Button -->
              <button 
                type="button" 
                class="btn btn-sm btn-light bg-body border rounded-2 px-2.5 py-1.5 text-secondary d-inline-flex align-items-center gap-1.5"
                @click="clearSelection"
                :title="$t('files.SCR0469')"
              >
                <i class="bi bi-x-lg"></i> <span>{{ $t('files.SCR0469') }}</span>
              </button>
            </div>
          </div>

          <!-- Loading State -->
          <div v-if="loadingExplorer" class="text-center py-5 my-auto">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="text-muted small mt-2">{{ $t('files.SCR0470') }}</p>
          </div>

          <!-- Empty State -->
          <div v-else-if="filteredFolders.length === 0 && filteredFiles.length === 0" class="text-center py-5 my-auto">
            <div class="empty-icon-box bg-body-secondary text-muted rounded-circle mx-auto mb-3 d-flex align-items-center justify-content-center" style="width: 64px; height: 64px;">
              <i class="bi bi-folder2-open fs-2"></i>
            </div>
            <h6 class="fw-bold text-body mb-1">{{ $t('files.SCR0416') }}</h6>
            <p class="text-muted small mb-3" style="max-width: 420px; margin: 0 auto;">{{ $t('files.SCR0417') }}</p>
            <div class="d-flex align-items-center justify-content-center gap-2">
              <button type="button" class="btn btn-sm btn-primary rounded-3 px-3 py-1.5 d-inline-flex align-items-center gap-2" @click="triggerFileInput">
                <i class="bi bi-cloud-arrow-up-fill"></i> <span>{{ $t('files.SCR0403') }}</span>
              </button>
              <button type="button" class="btn btn-sm btn-light bg-body-secondary border rounded-3 px-3 py-1.5 d-inline-flex align-items-center gap-2" @click="openCreateFolderModal">
                <i class="bi bi-folder-plus text-warning-emphasis"></i> <span>{{ $t('files.SCR0404') }}</span>
              </button>
            </div>
          </div>

          <!-- TABLE VIEW (Backlog Style with High-Visibility Checkboxes & Google Drive Previews) -->
          <div v-else-if="viewMode === 'table'" class="table-responsive">
            <table class="table table-hover align-middle border-0 mb-0">
              <thead class="table-light">
                <tr>
                  <!-- Checkbox Column -->
                  <th scope="col" class="border-0 rounded-start text-center px-2 py-2.5" style="width: 44px;">
                    <input 
                      class="form-check-input custom-file-checkbox" 
                      type="checkbox" 
                      :checked="isAllSelected"
                      :indeterminate="isIndeterminate"
                      @change="toggleSelectAll"
                      :title="$t('files.SCR0471')"
                    />
                  </th>
                  <th scope="col" class="border-0 text-start px-3 py-2.5">{{ $t('files.SCR0418') }}</th>
                  <th scope="col" class="border-0 text-start px-3 py-2.5" style="width: 100px;">{{ $t('files.SCR0419') }}</th>
                  <th scope="col" class="border-0 text-start px-3 py-2.5" style="min-width: 180px;">{{ $t('files.SCR0420') }}</th>
                  <th scope="col" class="border-0 text-start px-3 py-2.5" style="width: 110px;">{{ $t('files.SCR0421') }}</th>
                  <th scope="col" class="border-0 rounded-end text-end px-3 py-2.5" style="width: 130px;">{{ $t('files.SCR0422') }}</th>
                </tr>
              </thead>
              <tbody>
                <!-- Parent Folder Up Row (if in subfolder) -->
                <tr v-if="currentFolderId" class="folder-row cursor-pointer table-subtle" @click="navigateUp">
                  <td class="px-2 py-2 text-center text-muted">—</td>
                  <td colspan="5" class="px-3 py-2 text-primary fw-medium small">
                    <i class="bi bi-arrow-90deg-up me-2"></i>
                    <span>.. ({{ $t('files.SCR0472') }})</span>
                  </td>
                </tr>

                <!-- FOLDERS LIST -->
                <tr 
                  v-for="folder in filteredFolders" 
                  :key="folder.Id" 
                  class="folder-row cursor-pointer"
                  :class="{ 'table-active selected-file-row': selectedFolderIds.includes(folder.Id) }"
                  @click="navigateToFolder(folder.Id)"
                >
                  <!-- Row Checkbox -->
                  <td class="px-2 py-2.5 text-center" @click.stop>
                    <input 
                      type="checkbox" 
                      class="form-check-input custom-file-checkbox" 
                      :value="folder.Id"
                      v-model="selectedFolderIds"
                    />
                  </td>
                  <td class="px-3 py-2.5">
                    <div class="d-flex align-items-center gap-3">
                      <div class="folder-icon-box rounded-2 d-flex align-items-center justify-content-center bg-warning-subtle text-warning-emphasis flex-shrink-0">
                        <i class="bi bi-folder-fill fs-5"></i>
                      </div>
                      <div class="text-truncate" style="max-width: 280px;">
                        <span class="fw-semibold text-body d-block text-truncate folder-name-link" :title="folder.Name">{{ folder.Name }}</span>
                        <span class="text-muted small" style="font-size: 11px;">
                          {{ $t('files.SCR0474', { count: folder.FileCount }) }} <span v-if="folder.SubFolderCount > 0">· {{ $t('files.SCR0504', { count: folder.SubFolderCount }) }}</span>
                        </span>
                      </div>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-muted small font-monospace">—</td>
                  <td class="px-3 py-2.5">
                    <div class="d-flex align-items-center gap-2" style="white-space: nowrap;">
                      <div class="user-avatar-tiny rounded-circle bg-warning-subtle text-warning-emphasis d-flex align-items-center justify-content-center fw-bold flex-shrink-0" style="width: 24px; height: 24px; font-size: 11px;">
                        {{ folder.CreatedByName ? folder.CreatedByName[0].toUpperCase() : 'U' }}
                      </div>
                      <span class="small text-body fw-medium">{{ folder.CreatedByName }}</span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-muted small" style="white-space: nowrap;">
                    <div class="d-flex flex-column text-start" style="line-height: 1.25;">
                      <span class="text-body fw-medium" style="font-size: 12px;">{{ formatTimeOnly(folder.CreatedAt) }}</span>
                      <span class="text-secondary" style="font-size: 11px;">{{ formatDateOnly(folder.CreatedAt) }}</span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-end" @click.stop>
                    <div class="d-flex align-items-center justify-content-end gap-1">
                      <!-- Rename Folder -->
                      <button 
                        class="btn btn-sm btn-light border-0 text-secondary rounded-2 px-2 py-1" 
                        @click="openRenameModal(folder, true)"
                        :title="$t('files.SCR0427')"
                      >
                        <i class="bi bi-pencil"></i>
                      </button>

                      <!-- Delete Folder -->
                      <button 
                        v-if="canDeleteFile" 
                        class="btn btn-sm btn-light border-0 text-danger rounded-2 px-2 py-1" 
                        @click="handleDeleteFolder(folder)"
                        :title="$t('files.SCR0424')"
                      >
                        <i class="bi bi-trash3"></i>
                      </button>
                    </div>
                  </td>
                </tr>

                <!-- FILES LIST -->
                <tr 
                  v-for="file in filteredFiles" 
                  :key="file.Id" 
                  class="file-row"
                  :class="{ 'table-active selected-file-row': selectedFileIds.includes(file.Id) }"
                >
                  <!-- Row Checkbox -->
                  <td class="px-2 py-2.5 text-center" @click.stop>
                    <input 
                      type="checkbox" 
                      class="form-check-input custom-file-checkbox" 
                      :value="file.Id"
                      v-model="selectedFileIds"
                    />
                  </td>
                  <td class="px-3 py-2.5">
                    <div class="d-flex align-items-center gap-3">
                      <div class="file-icon-box rounded-2 d-flex align-items-center justify-content-center flex-shrink-0" :class="getFileIconClass(file.FileName)">
                        <i :class="getFileIcon(file.FileName)"></i>
                      </div>
                      <div class="text-truncate" style="max-width: 280px;">
                        <div class="d-flex align-items-center gap-2">
                          <!-- Preview is loaded through the authorized backend endpoint. -->
                          <span 
                            class="fw-semibold text-body text-truncate cursor-pointer file-link d-inline-flex align-items-center gap-1.5" 
                            :title="$t('files.SCR0476', { name: file.FileName })" 
                            @click="handleOpenFilePreview(file)"
                          >
                            <span class="text-truncate">{{ file.FileName }}</span>
                            <i class="bi bi-box-arrow-up-right small text-primary opacity-75 ms-1" style="font-size: 10px;"></i>
                          </span>
                          <span class="badge bg-primary-subtle text-primary border border-primary-subtle font-monospace py-0.5 px-1.5" style="font-size: 10px;">
                            v{{ file.CurrentVersion || 1 }}
                          </span>
                        </div>
                        <span v-if="file.TaskTitle" class="badge bg-secondary-subtle text-secondary small py-0.5 px-1.5 mt-0.5" style="font-size: 10px;">
                          <i class="bi bi-check-circle me-1"></i>{{ file.TaskTitle }}
                        </span>
                      </div>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-muted small font-monospace">{{ formatBytes(file.FileSize) }}</td>
                  <td class="px-3 py-2.5">
                    <div class="d-flex align-items-center gap-2" style="white-space: nowrap;">
                      <div class="user-avatar-tiny rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold flex-shrink-0" style="width: 24px; height: 24px; font-size: 11px;">
                        {{ (file.UpdatedByName || file.UploadedByName) ? (file.UpdatedByName || file.UploadedByName)[0].toUpperCase() : 'U' }}
                      </div>
                      <span class="small text-body fw-medium" :title="file.UploadedByEmail">{{ file.UpdatedByName || file.UploadedByName }}</span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-muted small" style="white-space: nowrap;">
                    <div class="d-flex flex-column text-start" style="line-height: 1.25;">
                      <span class="text-body fw-medium" style="font-size: 12px;">{{ formatTimeOnly(file.UpdatedAt || file.CreatedAt) }}</span>
                      <span class="text-secondary" style="font-size: 11px;">{{ formatDateOnly(file.UpdatedAt || file.CreatedAt) }}</span>
                    </div>
                  </td>
                  <td class="px-3 py-2.5 text-end">
                    <div class="d-flex align-items-center justify-content-end gap-1">
                      <!-- Open Preview on Google Drive Button -->
                      <button 
                        class="btn btn-sm btn-light border-0 text-success rounded-2 px-2 py-1" 
                        @click="handleOpenFilePreview(file)"
                        :title="$t('files.SCR0475')"
                      >
                        <i class="bi bi-eye"></i>
                      </button>

                      <!-- Version History Button -->
                      <button 
                        class="btn btn-sm btn-light border-0 text-secondary rounded-2 px-2 py-1" 
                        @click="openHistoryModal(file)"
                        :title="$t('files.SCR0432')"
                      >
                        <i class="bi bi-clock-history"></i>
                      </button>

                      <!-- Rename File -->
                      <button 
                        class="btn btn-sm btn-light border-0 text-secondary rounded-2 px-2 py-1" 
                        @click="openRenameModal(file, false)"
                        :title="$t('files.SCR0426')"
                      >
                        <i class="bi bi-pencil"></i>
                      </button>

                      <!-- Delete File Button -->
                      <button 
                        v-if="canDeleteFile" 
                        class="btn btn-sm btn-light border-0 text-danger rounded-2 px-2 py-1" 
                        @click="handleDeleteFile(file)"
                        :disabled="deletingFileId === file.Id"
                        :title="$t('files.SCR0424')"
                      >
                        <i v-if="deletingFileId === file.Id" class="bi bi-arrow-repeat spin"></i>
                        <i v-else class="bi bi-trash3"></i>
                      </button>
                      <button 
                        v-else
                        class="btn btn-sm btn-light border-0 text-muted rounded-2 px-2 py-1 opacity-25" 
                        disabled
                        :title="$t('files.SCR0452')"
                      >
                        <i class="bi bi-lock-fill"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- GRID VIEW -->
          <div v-else class="row g-3">
            <!-- Folders in Grid -->
            <div 
              v-for="folder in filteredFolders" 
              :key="folder.Id" 
              class="col-12 col-sm-6 col-md-4"
            >
              <div 
                class="card h-100 border rounded-3 p-3 folder-card cursor-pointer transition-all position-relative"
                :class="{ 'border-primary shadow-sm selected-card': selectedFolderIds.includes(folder.Id) }"
                @click="navigateToFolder(folder.Id)"
              >
                <!-- Checkbox on card top-left -->
                <div class="position-absolute top-0 start-0 m-2.5 z-2" @click.stop>
                  <input 
                    type="checkbox" 
                    class="form-check-input custom-file-checkbox shadow-sm" 
                    :value="folder.Id"
                    v-model="selectedFolderIds"
                  />
                </div>

                <div class="d-flex align-items-start justify-content-between mb-2 ms-4">
                  <div class="folder-icon-box-lg rounded-3 d-flex align-items-center justify-content-center bg-warning-subtle text-warning-emphasis">
                    <i class="bi bi-folder-fill fs-3"></i>
                  </div>
                  <div class="dropdown" @click.stop>
                    <button class="btn btn-sm btn-light border-0 p-1 rounded-2 text-muted" data-bs-toggle="dropdown">
                      <i class="bi bi-three-dots-vertical"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0 rounded-3 p-1">
                      <li>
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="openRenameModal(folder, true)">
                          <i class="bi bi-pencil text-secondary"></i>
                          <span>{{ $t('files.SCR0425') }}</span>
                        </button>
                      </li>
                      <li v-if="canDeleteFile">
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2 text-danger" @click="handleDeleteFolder(folder)">
                          <i class="bi bi-trash3"></i>
                          <span>{{ $t('files.SCR0424') }}</span>
                        </button>
                      </li>
                    </ul>
                  </div>
                </div>
                <h6 class="fw-semibold text-body mb-1 text-truncate" :title="folder.Name">{{ folder.Name }}</h6>
                <span class="text-muted small d-block mb-3" style="font-size: 11px;">{{ $t('files.SCR0474', { count: folder.FileCount }) }}</span>
                <div class="pt-2 border-top d-flex align-items-center justify-content-between mt-auto text-muted small" style="font-size: 10px;">
                  <span>{{ folder.CreatedByName }}</span>
                  <span>{{ formatDate(folder.CreatedAt) }}</span>
                </div>
              </div>
            </div>

            <!-- Files in Grid -->
            <div 
              v-for="file in filteredFiles" 
              :key="file.Id" 
              class="col-12 col-sm-6 col-md-4"
            >
              <div 
                class="card h-100 border rounded-3 p-3 file-card transition-all position-relative"
                :class="{ 'border-primary shadow-sm selected-card': selectedFileIds.includes(file.Id) }"
              >
                <!-- Checkbox on card top-left -->
                <div class="position-absolute top-0 start-0 m-2.5 z-2" @click.stop>
                  <input 
                    type="checkbox" 
                    class="form-check-input custom-file-checkbox shadow-sm" 
                    :value="file.Id"
                    v-model="selectedFileIds"
                  />
                </div>

                <div class="d-flex align-items-start justify-content-between mb-2 ms-4">
                  <div class="file-icon-box-lg rounded-3 d-flex align-items-center justify-content-center" :class="getFileIconClass(file.FileName)">
                    <i :class="getFileIcon(file.FileName)"></i>
                  </div>
                  <div class="dropdown">
                    <button class="btn btn-sm btn-light border-0 p-1 rounded-2 text-muted" data-bs-toggle="dropdown">
                      <i class="bi bi-three-dots-vertical"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0 rounded-3 p-1" style="min-width: 180px;">
                      <li>
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2 text-success" @click="handleOpenFilePreview(file)">
                          <i class="bi bi-eye"></i>
                          <span>{{ $t('files.SCR0475') }}</span>
                        </button>
                      </li>
                      <li>
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="handleDownload(file)">
                          <i class="bi bi-download text-primary"></i>
                          <span>{{ $t('files.SCR0423') }}</span>
                        </button>
                      </li>
                      <li>
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="openHistoryModal(file)">
                          <i class="bi bi-clock-history"></i>
                          <span>{{ $t('files.SCR0432') }}</span>
                        </button>
                      </li>
                      <li>
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2" @click="openRenameModal(file, false)">
                          <i class="bi bi-pencil"></i>
                          <span>{{ $t('files.SCR0425') }}</span>
                        </button>
                      </li>
                      <li v-if="canDeleteFile">
                        <button class="dropdown-item d-flex align-items-center gap-2 py-2 rounded-2 text-danger" @click="handleDeleteFile(file)">
                          <i class="bi bi-trash3"></i>
                          <span>{{ $t('files.SCR0424') }}</span>
                        </button>
                      </li>
                    </ul>
                  </div>
                </div>

                <div class="d-flex align-items-center gap-1.5 mb-1">
                  <!-- Click title opens Google Drive Preview -->
                  <h6 
                    class="fw-semibold text-body mb-0 text-truncate cursor-pointer file-link d-inline-flex align-items-center gap-1" 
                    :title="$t('files.SCR0476', { name: file.FileName })"
                    @click="handleOpenFilePreview(file)"
                  >
                    <span class="text-truncate">{{ file.FileName }}</span>
                    <i class="bi bi-box-arrow-up-right small text-primary opacity-75 ms-0.5" style="font-size: 10px;"></i>
                  </h6>
                  <span class="badge bg-primary-subtle text-primary border border-primary-subtle font-monospace py-0.5 px-1.5" style="font-size: 10px;">
                    v{{ file.CurrentVersion || 1 }}
                  </span>
                </div>

                <div class="d-flex align-items-center justify-content-between text-muted small font-monospace mb-2" style="font-size: 11px;">
                  <span>{{ formatBytes(file.FileSize) }}</span>
                  <span>{{ formatDate(file.UpdatedAt || file.CreatedAt) }}</span>
                </div>

                <div class="pt-2 border-top d-flex align-items-center justify-content-between mt-auto">
                  <span class="small text-muted text-truncate" style="font-size: 11px; max-width: 110px;">
                    {{ file.UpdatedByName || file.UploadedByName }}
                  </span>
                  <div class="d-flex align-items-center gap-1">
                    <button 
                      class="btn btn-sm btn-light border-0 text-success rounded-2 px-2 py-0.5" 
                      @click="handleOpenFilePreview(file)"
                      :title="$t('files.SCR0475')"
                    >
                      <i class="bi bi-eye"></i>
                    </button>
                    <button 
                      class="btn btn-sm btn-primary-subtle text-primary border-0 rounded-2 px-2 py-0.5" 
                      @click="handleDownload(file)"
                      :disabled="downloadingFileId === file.Id"
                      :title="$t('files.SCR0423')"
                    >
                      <i v-if="downloadingFileId === file.Id" class="bi bi-arrow-repeat spin"></i>
                      <i v-else class="bi bi-download"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- ==================== RIGHT COLUMN: ELEGANT & CLEAN ACTIVITY FEED (2 Parts) ==================== -->
      <div class="col-12 col-xl-3 col-lg-4 d-flex flex-column">
        <div class="card border-0 shadow-sm p-3 rounded-3 bg-body h-100 d-flex flex-column">
          
          <!-- Clean Header -->
          <div class="d-flex align-items-center justify-content-between pb-3 mb-2 border-bottom">
            <div class="d-flex align-items-center gap-2">
              <i class="bi bi-clock-history text-primary"></i>
              <h6 class="fw-bold mb-0 text-body" style="font-size: 14px;">
                {{ $t('files.SCR0406') }}
              </h6>
              <span class="badge bg-body-secondary text-secondary rounded-pill font-monospace" style="font-size: 10.5px;">
                {{ activities.length }}
              </span>
            </div>

            <button 
              type="button" 
              class="btn btn-sm btn-light bg-body-secondary border rounded-2 p-1 text-muted" 
              @click="fetchActivities" 
              :title="$t('files.SCR0477')"
              :disabled="loadingActivities"
            >
              <i class="bi bi-arrow-clockwise" :class="{ spin: loadingActivities }"></i>
            </button>
          </div>

          <!-- Loading State -->
          <div v-if="loadingActivities && activities.length === 0" class="text-center py-5 my-auto">
            <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
            <p class="text-muted small mt-2" style="font-size: 12px;">{{ $t('common.SCR0009') }}</p>
          </div>

          <!-- Empty State -->
          <div v-else-if="activities.length === 0" class="text-center py-5 text-muted small my-auto">
            <i class="bi bi-journal-text fs-3 d-block mb-1 text-muted opacity-50"></i>
            <span>{{ $t('files.SCR0458') }}</span>
          </div>

          <!-- Clean Single-Flow Activity List -->
          <div v-else class="overflow-auto flex-grow-1 pe-1 clean-activity-feed d-flex flex-column gap-2 pt-1">
            <div 
              v-for="(act, idx) in activities" 
              :key="act.Id" 
              class="d-flex align-items-start gap-2.5 p-2.5 rounded-3 clean-activity-item"
              :class="idx < activities.length - 1 ? 'border-bottom border-light-subtle pb-3 mb-1' : ''"
            >
              <!-- Action Icon Badge -->
              <div 
                class="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0 mt-0.5 border"
                :class="getActionIconBgClass(act.ActionType)"
                style="width: 28px; height: 28px; font-size: 12px;"
              >
                <i :class="getActionIcon(act.ActionType)"></i>
              </div>

              <!-- Content Body -->
              <div class="flex-grow-1 min-w-0" style="font-size: 12.5px; line-height: 1.45;">
                <div class="text-body">
                  <span class="fw-semibold text-body">{{ act.UserName }}</span>
                  <span class="text-secondary mx-1">{{ getActionVerb(act.ActionType) }}</span>
                  <span class="fw-semibold text-body text-break">{{ act.TargetName }}</span>
                </div>

                <!-- Timestamp -->
                <div class="text-muted small mt-1 font-monospace" style="font-size: 11px;">
                  <i class="bi bi-clock me-1 opacity-75"></i>{{ timeAgo(act.CreatedAt) }}
                </div>

                <!-- Sub-detail text if available -->
                <div v-if="act.Details" class="text-muted small mt-1.5 ps-2 border-start border-2 border-primary-subtle" style="font-size: 11.5px; font-style: italic;">
                  {{ act.Details }}
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>

    </div>

    <!-- ==================== MODALS ==================== -->

    <!-- MODAL: CREATE FOLDER -->
    <div v-if="showCreateFolderModal" class="modal-backdrop-custom d-flex align-items-center justify-content-center">
      <div class="modal-card bg-body rounded-3 shadow-lg p-4" style="width: 420px; max-width: 90%;">
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h5 class="fw-bold mb-0 text-body d-flex align-items-center gap-2">
            <i class="bi bi-folder-plus text-warning-emphasis"></i>
            {{ $t('files.SCR0435') }}
          </h5>
          <button type="button" class="btn-close" @click="showCreateFolderModal = false"></button>
        </div>
        <form @submit.prevent="submitCreateFolder">
          <div class="mb-3">
            <label class="form-label small fw-medium text-secondary">{{ $t('files.SCR0436') }}</label>
            <input 
              v-model="newFolderName" 
              type="text" 
              class="form-control rounded-3" 
              :placeholder="$t('files.SCR0437')" 
              required
              autofocus
            />
          </div>
          <div class="d-flex justify-content-end gap-2">
            <button type="button" class="btn btn-light rounded-2 px-3" @click="showCreateFolderModal = false">
              {{ $t('files.SCR0445') }}
            </button>
            <button type="submit" class="btn btn-primary rounded-2 px-3 fw-semibold" :disabled="submittingFolder">
              <span v-if="submittingFolder" class="spinner-border spinner-border-sm me-1"></span>
              {{ $t('files.SCR0404') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- MODAL: RENAME FILE OR FOLDER -->
    <div v-if="showRenameModal" class="modal-backdrop-custom d-flex align-items-center justify-content-center">
      <div class="modal-card bg-body rounded-3 shadow-lg p-4" style="width: 420px; max-width: 90%;">
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h5 class="fw-bold mb-0 text-body">
            {{ isRenamingFolder ? $t('files.SCR0427') : $t('files.SCR0426') }}
          </h5>
          <button type="button" class="btn-close" @click="showRenameModal = false"></button>
        </div>
        <form @submit.prevent="submitRename">
          <div class="mb-3">
            <label class="form-label small fw-medium text-secondary">{{ $t('files.SCR0418') }}</label>
            <input 
              v-model="renameValue" 
              type="text" 
              class="form-control rounded-3" 
              required
              autofocus
            />
          </div>
          <div class="d-flex justify-content-end gap-2">
            <button type="button" class="btn btn-light rounded-2 px-3" @click="showRenameModal = false">
              {{ $t('files.SCR0445') }}
            </button>
            <button type="submit" class="btn btn-primary rounded-2 px-3 fw-semibold" :disabled="submittingRename">
              <span v-if="submittingRename" class="spinner-border spinner-border-sm me-1"></span>
              {{ $t('files.SCR0425') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- MODAL: FILE VERSION HISTORY -->
    <div v-if="showHistoryModal" class="modal-backdrop-custom d-flex align-items-center justify-content-center">
      <div class="modal-card bg-body rounded-3 shadow-lg p-4 d-flex flex-column" style="width: 580px; max-width: 92%; max-height: 85vh;">
        <div class="d-flex align-items-center justify-content-between pb-3 mb-3 border-bottom">
          <div>
            <h5 class="fw-bold mb-0 text-body d-flex align-items-center gap-2">
              <i class="bi bi-clock-history text-primary"></i>
              {{ $t('files.SCR0432') }}
            </h5>
            <span class="small text-muted text-truncate d-block mt-0.5" style="max-width: 440px;">{{ selectedFileForHistory?.FileName }}</span>
          </div>
          <button type="button" class="btn-close" @click="showHistoryModal = false"></button>
        </div>

        <div v-if="loadingHistory" class="text-center py-4 my-auto">
          <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
        </div>

        <div v-else class="overflow-auto pe-2 flex-grow-1">
          <div class="timeline-container">
            <div 
              v-for="v in fileVersions" 
              :key="v.Id" 
              class="timeline-item p-3 mb-2.5 rounded-3 border bg-body-secondary transition-all"
            >
              <div class="d-flex align-items-start justify-content-between gap-2 mb-1.5">
                <div class="d-flex align-items-center gap-2">
                  <span class="badge bg-primary text-white font-monospace px-2 py-0.5" style="font-size: 11px;">
                    v{{ v.VersionNumber }}
                  </span>
                  <span class="fw-semibold text-body small text-truncate" style="max-width: 260px;">{{ v.FileName }}</span>
                </div>
                <div class="d-flex align-items-center gap-1.5">
                  <!-- Open version through the authorized backend endpoint. -->
                  <button 
                    class="btn btn-sm btn-light bg-body border-0 rounded-2 px-2 py-0.5 fw-medium small text-success"
                    @click="handleOpenVersionPreview(v)"
                    :title="$t('files.SCR0478')"
                  >
                    <i class="bi bi-eye me-1"></i> {{ $t('files.SCR0479') }}
                  </button>
                  <!-- Download version -->
                  <button 
                    class="btn btn-sm btn-primary-subtle text-primary border-0 rounded-2 px-2 py-0.5 fw-medium small"
                    @click="handleDownloadVersion(selectedFileForHistory, v)"
                    :title="$t('files.SCR0480')"
                  >
                    <i class="bi bi-download me-1"></i> {{ $t('files.SCR0423') }}
                  </button>
                </div>
              </div>

              <div class="d-flex align-items-center justify-content-between text-muted small font-monospace mb-1.5" style="font-size: 11px;">
                <span>{{ formatBytes(v.FileSize) }} · {{ v.UploadedByName }}</span>
                <span>{{ formatDate(v.CreatedAt) }}</span>
              </div>

              <p v-if="v.ChangeNote" class="small text-secondary mb-0 bg-body p-2 rounded-2 border">
                <i class="bi bi-chat-left-text me-1.5 text-muted"></i>{{ v.ChangeNote }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useProjectStore } from '../stores/projectStore.js'
import { 
  getProjectExplorer, 
  uploadProjectFile, 
  getFileVersionHistory, 
  downloadProjectFile, 
  batchDownloadProjectFiles,
  batchDeleteProjectFiles,
  renameProjectFile, 
  deleteProjectFile, 
  createProjectFolder, 
  renameProjectFolder, 
  deleteProjectFolder, 
  getProjectFileActivities 
} from '../services/projectService.js'
import { useI18n } from 'vue-i18n'
import { toastSuccess, toastError, confirm, extractMessage } from '../utils/swal.js'

const { t, locale } = useI18n()
const route = useRoute()
const projectStore = useProjectStore()

const currentFolderId = ref(null)
const breadcrumbs = ref([])
const folders = ref([])
const files = ref([])
const loadingExplorer = ref(false)
const searchQuery = ref('')
const viewMode = ref('table')
const driveConfigWarning = ref(false)

// Checkbox selection state
const selectedFolderIds = ref([])
const selectedFileIds = ref([])
const isBatchDownloading = ref(false)
const isBatchDeleting = ref(false)

// Activities state (for right-hand panel)
const activities = ref([])
const loadingActivities = ref(false)

// Drag & drop upload state
const isUploading = ref(false)
const uploadProgress = ref(0)
const uploadingFileName = ref('')
const fileInputRef = ref(null)

// Download & delete state
const downloadingFileId = ref(null)
const deletingFileId = ref(null)

// Modals state
const showCreateFolderModal = ref(false)
const newFolderName = ref('')
const submittingFolder = ref(false)

const showRenameModal = ref(false)
const itemToRename = ref(null)
const isRenamingFolder = ref(false)
const renameValue = ref('')
const submittingRename = ref(false)

const showHistoryModal = ref(false)
const selectedFileForHistory = ref(null)
const fileVersions = ref([])
const loadingHistory = ref(false)

const canDeleteFile = computed(() => {
  return projectStore.appRole === 'Admin' || 
         projectStore.userRole === 'Owner' || 
         projectStore.userRole === 'Manager'
})

const projectId = computed(() => route.params.projectId)

const filteredFolders = computed(() => {
  if (!searchQuery.value) return folders.value
  return folders.value.filter(f => f.Name.toLowerCase().includes(searchQuery.value.toLowerCase()))
})

const filteredFiles = computed(() => {
  if (!searchQuery.value) return files.value
  return files.value.filter(file => 
    file.FileName.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

// Selection computed properties
const totalItemsCount = computed(() => filteredFolders.value.length + filteredFiles.value.length)
const selectedCount = computed(() => selectedFolderIds.value.length + selectedFileIds.value.length)

const isAllSelected = computed(() => {
  if (totalItemsCount.value === 0) return false
  return selectedFolderIds.value.length === filteredFolders.value.length && 
         selectedFileIds.value.length === filteredFiles.value.length
})

const isIndeterminate = computed(() => {
  return selectedCount.value > 0 && !isAllSelected.value
})

const isZipBatch = computed(() => {
  return selectedCount.value > 1 || selectedFolderIds.value.length > 0
})

const toggleSelectAll = () => {
  if (isAllSelected.value) {
    clearSelection()
  } else {
    selectedFolderIds.value = filteredFolders.value.map(f => f.Id)
    selectedFileIds.value = filteredFiles.value.map(f => f.Id)
  }
}

const clearSelection = () => {
  selectedFolderIds.value = []
  selectedFileIds.value = []
}

const fetchExplorer = async () => {
  if (!projectId.value) return
  loadingExplorer.value = true
  driveConfigWarning.value = false
  try {
    const res = await getProjectExplorer(projectId.value, currentFolderId.value)
    const data = res.data || {}
    breadcrumbs.value = data.Breadcrumbs || data.breadcrumbs || []
    folders.value = (data.Folders || data.folders || []).map(f => ({
      Id: f.Id || f.id,
      ProjectId: f.ProjectId || f.projectId,
      ParentFolderId: f.ParentFolderId || f.parentFolderId,
      Name: f.Name || f.name,
      CreatedById: f.CreatedById || f.createdById,
      CreatedByName: f.CreatedByName || f.createdByName || t('files.SCR0482'),
      CreatedAt: f.CreatedAt || f.createdAt || new Date().toISOString(),
      FileCount: f.FileCount || f.fileCount || 0,
      SubFolderCount: f.SubFolderCount || f.subFolderCount || 0
    }))
    files.value = (data.Files || data.files || []).map(f => ({
      Id: f.Id || f.id,
      ProjectId: f.ProjectId || f.projectId,
      FolderId: f.FolderId || f.folderId,
      TaskId: f.TaskId || f.taskId,
      TaskTitle: f.TaskTitle || f.taskTitle,
      FileName: f.FileName || f.fileName || t('files.SCR0481'),
      FileSize: f.FileSize || f.fileSize || 0,
      MimeType: f.MimeType || f.mimeType || 'application/octet-stream',
      CurrentVersion: f.CurrentVersion || f.currentVersion || 1,
      UploadedById: f.UploadedById || f.uploadedById,
      UploadedByName: f.UploadedByName || f.uploadedByName || t('files.SCR0482'),
      UploadedByEmail: f.UploadedByEmail || f.uploadedByEmail || '',
      CreatedAt: f.CreatedAt || f.createdAt || new Date().toISOString(),
      UpdatedAt: f.UpdatedAt || f.updatedAt,
      UpdatedByName: f.UpdatedByName || f.updatedByName
    }))
  } catch (error) {
    console.error('Error fetching explorer:', error)
    if (error.response?.data?.Message?.includes('credentials') || error.response?.data?.message?.includes('credentials')) {
      driveConfigWarning.value = true
    }
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loadingExplorer.value = false
  }
}

const fetchActivities = async () => {
  if (!projectId.value) return
  loadingActivities.value = true
  try {
    const res = await getProjectFileActivities(projectId.value)
    activities.value = res.data || []
  } catch (error) {
    console.error('Fetch activities error:', error)
  } finally {
    loadingActivities.value = false
  }
}

// Navigation
const navigateToFolder = (folderId) => {
  clearSelection()
  currentFolderId.value = folderId
  fetchExplorer()
}

const navigateUp = () => {
  clearSelection()
  if (breadcrumbs.value.length <= 1) {
    currentFolderId.value = null
  } else {
    const parent = breadcrumbs.value[breadcrumbs.value.length - 2]
    currentFolderId.value = parent.Id
  }
  fetchExplorer()
}

// File Upload
const triggerFileInput = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click()
  }
}

const handleFileInputChange = async (e) => {
  const file = e.target.files?.[0]
  if (!file) return

  if (file.size > 52428800) {
    toastError(t('files.SCR0409'))
    return
  }

  isUploading.value = true
  uploadProgress.value = 0
  uploadingFileName.value = file.name

  try {
    await uploadProjectFile(
      projectId.value, 
      file, 
      currentFolderId.value, 
      null, 
      (progressEvent) => {
        if (progressEvent.total) {
          uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        }
      }
    )

    toastSuccess(t('files.SCR0447'))
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('File upload error:', error)
    const msg = extractMessage(error, t('files.SCR0450'))
    toastError(msg)
    if (msg.includes('credentials') || msg.includes('chưa được cấu hình')) {
      driveConfigWarning.value = true
    }
  } finally {
    isUploading.value = false
    uploadProgress.value = 0
    uploadingFileName.value = ''
    if (fileInputRef.value) fileInputRef.value.value = ''
  }
}

const openSecurePreview = async (file, versionId = null) => {
  if (!file?.Id) {
    toastError(t('files.SCR0483'))
    return
  }

  // Open synchronously so popup protection does not block the tab while the
  // authenticated backend request is in progress.
  const previewWindow = window.open('', '_blank')

  try {
    const response = await downloadProjectFile(projectId.value, file.Id, versionId)
    const blob = new Blob([response.data], {
      type: response.headers?.['content-type'] || file.MimeType || 'application/octet-stream'
    })
    const blobUrl = window.URL.createObjectURL(blob)

    if (previewWindow) {
      previewWindow.opener = null
      previewWindow.location.replace(blobUrl)
    } else {
      window.open(blobUrl, '_blank', 'noopener,noreferrer')
    }

    window.setTimeout(() => window.URL.revokeObjectURL(blobUrl), 60_000)
  } catch (error) {
    previewWindow?.close()
    console.error('Secure file preview error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  }
}

const handleOpenFilePreview = async (file) => {
  await openSecurePreview(file)
}

const handleOpenVersionPreview = async (version) => {
  if (!selectedFileForHistory.value) {
    toastError(t('files.SCR0484'))
    return
  }
  await openSecurePreview(selectedFileForHistory.value, version.Id)
}

// Single Download
const handleDownload = async (file) => {
  downloadingFileId.value = file.Id
  try {
    const response = await downloadProjectFile(projectId.value, file.Id)
    const blob = new Blob([response.data], { type: file.MimeType || 'application/octet-stream' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', file.FileName)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    toastSuccess(`${t('files.SCR0423')}: ${file.FileName}`)
  } catch (error) {
    console.error('File download error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    downloadingFileId.value = null
  }
}

const handleDownloadVersion = async (file, version) => {
  try {
    const response = await downloadProjectFile(projectId.value, file.Id, version.Id)
    const blob = new Blob([response.data], { type: version.MimeType || 'application/octet-stream' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', version.FileName)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    toastSuccess(`${t('files.SCR0423')} (v${version.VersionNumber}): ${version.FileName}`)
  } catch (error) {
    console.error('Version download error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  }
}

// BATCH DOWNLOAD (Multiple files / Folders -> .zip, 1 file -> direct)
const handleBatchDownload = async () => {
  if (selectedCount.value === 0) return

  // If exactly 1 file and 0 folders selected, download directly
  if (selectedFileIds.value.length === 1 && selectedFolderIds.value.length === 0) {
    const singleFile = files.value.find(f => f.Id === selectedFileIds.value[0])
    if (singleFile) {
      await handleDownload(singleFile)
      return
    }
  }

  isBatchDownloading.value = true
  try {
    const res = await batchDownloadProjectFiles(projectId.value, selectedFileIds.value, selectedFolderIds.value)
    
    // Determine zip filename
    let zipName = `files_${new Date().toISOString().slice(0, 10)}.zip`
    const disposition = res.headers?.['content-disposition']
    if (disposition && disposition.includes('filename=')) {
      const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
      if (match && match[1]) {
        zipName = match[1].replace(/['"]/g, '')
      }
    }

    const blob = new Blob([res.data], { type: 'application/zip' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', zipName)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    toastSuccess(t('files.SCR0485', { count: selectedCount.value }))
  } catch (error) {
    console.error('Batch download error:', error)
    toastError(extractMessage(error, t('files.SCR0486')))
  } finally {
    isBatchDownloading.value = false
  }
}

// BATCH DELETE
const handleBatchDelete = async () => {
  if (selectedCount.value === 0) return

  const count = selectedCount.value
  const ok = await confirm(
    t('files.SCR0487'),
    t('files.SCR0488', { count }),
    t('files.SCR0489')
  )
  if (!ok) return

  isBatchDeleting.value = true
  try {
    await batchDeleteProjectFiles(projectId.value, selectedFileIds.value, selectedFolderIds.value)
    toastSuccess(t('files.SCR0490', { count }))
    clearSelection()
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('Batch delete error:', error)
    toastError(extractMessage(error, t('files.SCR0491')))
  } finally {
    isBatchDeleting.value = false
  }
}

// Folder Create
const openCreateFolderModal = () => {
  newFolderName.value = ''
  showCreateFolderModal.value = true
}

const submitCreateFolder = async () => {
  if (!newFolderName.value.trim()) return
  submittingFolder.value = true
  try {
    await createProjectFolder(projectId.value, newFolderName.value.trim(), currentFolderId.value)
    showCreateFolderModal.value = false
    toastSuccess(t('files.SCR0439'))
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('Create folder error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    submittingFolder.value = false
  }
}

// Rename
const openRenameModal = (item, isFolder) => {
  itemToRename.value = item
  isRenamingFolder.value = isFolder
  renameValue.value = isFolder ? item.Name : item.FileName
  showRenameModal.value = true
}

const submitRename = async () => {
  if (!renameValue.value.trim() || !itemToRename.value) return
  submittingRename.value = true
  try {
    if (isRenamingFolder.value) {
      await renameProjectFolder(projectId.value, itemToRename.value.Id, renameValue.value.trim())
    } else {
      await renameProjectFile(projectId.value, itemToRename.value.Id, renameValue.value.trim())
    }
    showRenameModal.value = false
    toastSuccess(t('files.SCR0428'))
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('Rename error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    submittingRename.value = false
    itemToRename.value = null
  }
}

// Version History
const openHistoryModal = async (file) => {
  selectedFileForHistory.value = file
  showHistoryModal.value = true
  loadingHistory.value = true
  try {
    const res = await getFileVersionHistory(projectId.value, file.Id)
    fileVersions.value = res.data || []
  } catch (error) {
    console.error('Fetch history error:', error)
    toastError(extractMessage(error, t('common.SCR0015')))
  } finally {
    loadingHistory.value = false
  }
}

// Delete Single File
const handleDeleteFile = async (file) => {
  if (!file) return
  const ok = await confirm(
    t('files.SCR0440'),
    t('files.SCR0441', { name: file.FileName }),
    t('files.SCR0444')
  )
  if (!ok) return

  deletingFileId.value = file.Id
  try {
    await deleteProjectFile(projectId.value, file.Id)
    toastSuccess(t('files.SCR0449'))
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('File delete error:', error)
    toastError(extractMessage(error, t('files.SCR0451')))
  } finally {
    deletingFileId.value = null
  }
}

const handleDeleteFolder = async (folder) => {
  if (!folder) return
  const ok = await confirm(
    t('files.SCR0442'),
    t('files.SCR0443', { name: folder.Name }),
    t('files.SCR0444')
  )
  if (!ok) return

  try {
    await deleteProjectFolder(projectId.value, folder.Id)
    toastSuccess(t('files.SCR0449'))
    await Promise.all([fetchExplorer(), fetchActivities()])
  } catch (error) {
    console.error('Folder delete error:', error)
    toastError(extractMessage(error, t('files.SCR0451')))
  }
}

// Helpers
const getFileExtension = (filename) => {
  if (!filename) return ''
  const parts = filename.split('.')
  return parts.length > 1 ? parts.pop() : ''
}

const getFileIcon = (filename) => {
  const ext = getFileExtension(filename).toLowerCase()
  switch (ext) {
    case 'pdf': return 'bi bi-file-earmark-pdf-fill'
    case 'doc':
    case 'docx': return 'bi bi-file-earmark-word-fill'
    case 'xls':
    case 'xlsx':
    case 'csv': return 'bi bi-file-earmark-excel-fill'
    case 'ppt':
    case 'pptx': return 'bi bi-file-earmark-ppt-fill'
    case 'jpg':
    case 'jpeg':
    case 'png':
    case 'gif':
    case 'webp':
    case 'svg': return 'bi bi-file-earmark-image-fill'
    case 'zip':
    case 'rar':
    case '7z':
    case 'tar': return 'bi bi-file-earmark-zip-fill'
    case 'txt': return 'bi bi-file-earmark-text-fill'
    case 'js':
    case 'ts':
    case 'json':
    case 'html':
    case 'css':
    case 'cs': return 'bi bi-file-earmark-code-fill'
    default: return 'bi bi-file-earmark-fill'
  }
}

const getFileIconClass = (filename) => {
  const ext = getFileExtension(filename).toLowerCase()
  switch (ext) {
    case 'pdf': return 'icon-pdf'
    case 'doc':
    case 'docx': return 'icon-word'
    case 'xls':
    case 'xlsx':
    case 'csv': return 'icon-excel'
    case 'ppt':
    case 'pptx': return 'icon-ppt'
    case 'jpg':
    case 'jpeg':
    case 'png':
    case 'gif':
    case 'webp':
    case 'svg': return 'icon-image'
    case 'zip':
    case 'rar':
    case '7z': return 'icon-zip'
    case 'txt': return 'icon-text'
    default: return 'icon-generic'
  }
}

// Action Helper: natural language verb
const getActionVerb = (action) => {
  switch (action) {
    case 'UploadFile': return t('files.SCR0492')
    case 'UpdateVersion': return t('files.SCR0493')
    case 'CreateFolder': return t('files.SCR0494')
    case 'RenameFile':
    case 'RenameFolder': return t('files.SCR0495')
    case 'DeleteFile':
    case 'DeleteFolder': return t('files.SCR0496')
    default: return t('files.SCR0497')
  }
}

const getActionIcon = (action) => {
  switch (action) {
    case 'UploadFile': return 'bi bi-cloud-arrow-up text-primary'
    case 'UpdateVersion': return 'bi bi-arrow-repeat text-success'
    case 'CreateFolder': return 'bi bi-folder-plus text-warning-emphasis'
    case 'RenameFile':
    case 'RenameFolder': return 'bi bi-pencil text-secondary'
    case 'DeleteFile':
    case 'DeleteFolder': return 'bi bi-trash3 text-danger'
    default: return 'bi bi-activity text-secondary'
  }
}

const getActionIconBgClass = (action) => {
  switch (action) {
    case 'UploadFile': return 'bg-primary-subtle border-primary-subtle'
    case 'UpdateVersion': return 'bg-success-subtle border-success-subtle'
    case 'CreateFolder': return 'bg-warning-subtle border-warning-subtle'
    case 'RenameFile':
    case 'RenameFolder': return 'bg-body-secondary border-secondary-subtle'
    case 'DeleteFile':
    case 'DeleteFolder': return 'bg-danger-subtle border-danger-subtle'
    default: return 'bg-body-secondary border-secondary-subtle'
  }
}

const timeAgo = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  const now = new Date()
  const diffInSeconds = Math.floor((now - date) / 1000)

  if (diffInSeconds < 30) return t('files.SCR0498')
  if (diffInSeconds < 60) return t('files.SCR0499', { count: diffInSeconds })
  
  const diffInMinutes = Math.floor(diffInSeconds / 60)
  if (diffInMinutes < 60) return t('files.SCR0500', { count: diffInMinutes })
  
  const diffInHours = Math.floor(diffInMinutes / 60)
  if (diffInHours < 24) return t('files.SCR0501', { count: diffInHours })
  
  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays === 1) return t('files.SCR0502')
  if (diffInDays < 7) return t('files.SCR0503', { count: diffInDays })
  
  return date.toLocaleDateString(locale.value === 'vi' ? 'vi-VN' : 'en-US', { day: '2-digit', month: '2-digit' })
}

const formatBytes = (bytes) => {
  if (!bytes || bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
}

const formatTimeOnly = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleTimeString(locale.value === 'vi' ? 'vi-VN' : 'en-US', { hour: '2-digit', minute: '2-digit', hour12: false })
}

const formatDateOnly = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleDateString(locale.value === 'vi' ? 'vi-VN' : 'en-US', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleDateString(locale.value === 'vi' ? 'vi-VN' : 'en-US', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  fetchExplorer()
  fetchActivities()
})

watch(() => route.params.projectId, (newVal) => {
  if (newVal) {
    clearSelection()
    currentFolderId.value = null
    fetchExplorer()
    fetchActivities()
  }
})
</script>

<style scoped>
.spin {
  animation: spin-animation 1s infinite linear;
  display: inline-block;
}

@keyframes spin-animation {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.cursor-pointer {
  cursor: pointer;
}

/* ==================== HIGH-CONTRAST & PRO CHECKBOXES ==================== */
.custom-file-checkbox {
  width: 18px !important;
  height: 18px !important;
  border: 1.8px solid #94a3b8 !important; /* Crisp Slate-400 border */
  border-radius: 4px !important;
  background-color: #ffffff !important;
  cursor: pointer !important;
  transition: all 0.15s ease-in-out !important;
  margin: 0 auto !important;
  display: inline-block !important;
  vertical-align: middle !important;
}

[data-bs-theme="dark"] .custom-file-checkbox {
  border-color: #64748b !important;
  background-color: #1e293b !important;
}

.custom-file-checkbox:hover {
  border-color: var(--bs-primary) !important;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.18) !important;
  transform: scale(1.05);
}

.custom-file-checkbox:checked {
  background-color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='%23fff' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='M5 10l3.5 3.5L15 6'/%3e%3c/svg%3e") !important;
  box-shadow: 0 2px 4px rgba(13, 110, 253, 0.25) !important;
}

.custom-file-checkbox:indeterminate {
  background-color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='%23fff' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='M6 10h8'/%3e%3c/svg%3e") !important;
}

/* Highlight selected row */
.selected-file-row {
  background-color: rgba(13, 110, 253, 0.05) !important;
}

.selected-card {
  border-color: var(--bs-primary) !important;
  background-color: rgba(13, 110, 253, 0.04) !important;
}

/* ==================== UNIFIED TOOLBAR CONTROLS ==================== */
.toolbar-control {
  height: 38px !important;
  font-size: 13.5px !important;
  box-sizing: border-box;
}

.toolbar-btn-group .btn {
  height: 100% !important;
  font-size: 13.5px !important;
  padding: 0 10px !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
}

.folder-icon-box {
  width: 34px;
  height: 34px;
}

.folder-icon-box-lg {
  width: 44px;
  height: 44px;
}

.file-icon-box {
  width: 34px;
  height: 34px;
  font-size: 16px;
}

.file-icon-box-lg {
  width: 44px;
  height: 44px;
  font-size: 22px;
}

.folder-name-link:hover,
.file-link:hover {
  color: var(--bs-primary) !important;
  text-decoration: underline;
}

.icon-pdf { background: #fee2e2; color: #dc2626; }
.icon-word { background: #dbeafe; color: #2563eb; }
.icon-excel { background: #dcfce7; color: #16a34a; }
.icon-ppt { background: #ffedd5; color: #ea580c; }
.icon-image { background: #f3e8ff; color: #9333ea; }
.icon-zip { background: #fef9c3; color: #ca8a04; }
.icon-text { background: #f1f5f9; color: #475569; }
.icon-generic { background: #e2e8f0; color: #64748b; }

.folder-row:hover,
.file-row:hover {
  background-color: var(--bs-secondary-bg) !important;
}

.folder-card,
.file-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  background: var(--bs-body-bg);
}

.folder-card:hover,
.file-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--bs-box-shadow-sm) !important;
  border-color: var(--bs-primary-border-subtle) !important;
}

/* ==================== CLEAN ACTIVITY FEED ==================== */
.clean-activity-feed {
  max-height: calc(100vh - 180px);
}

.clean-activity-item {
  transition: background-color 0.15s ease;
}

.clean-activity-item:hover {
  background-color: var(--bs-secondary-bg);
}

.modal-backdrop-custom {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.45);
  backdrop-filter: blur(4px);
  z-index: 1080;
  animation: fadeIn 0.15s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.transition-all {
  transition: all 0.2s ease;
}
</style>
