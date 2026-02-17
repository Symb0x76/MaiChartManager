import { STEP } from "@/components/MusicList/BatchActionButton/index";
import {
  currentProcessItem,
  progressAll,
  progressCurrent,
} from "@/components/MusicList/BatchActionButton/ProgressDisplay";
import { MusicXmlWithABJacket } from "@/client/apiGen";
import { BlobWriter, ZipReader } from "@zip.js/zip.js";
import getSubDirFile from "@/utils/getSubDirFile";
import {
  MAIDATA_SUBDIR,
  OPTIONS,
} from "@/components/MusicList/BatchActionButton/ChooseAction";
import { useNotification } from "naive-ui";
import { getUrl } from "@/client/api";
import { addVersionList, genreList } from "@/store/refs";
import { t } from "@/locales";
import { sanitizeFsSegment } from "@/utils/sanitizeFsName";

export default async (
  setStep: (step: STEP) => void,
  musicList: MusicXmlWithABJacket[],
  action: OPTIONS,
  notify: ReturnType<typeof useNotification>,
  dirOption: MAIDATA_SUBDIR,
) => {
  let folderHandle: FileSystemDirectoryHandle;
  try {
    folderHandle = await window.showDirectoryPicker({
      id: "copyToSaveDir",
      mode: "readwrite",
    });
  } catch (e) {
    console.log(e);
    return;
  }

  const getMaidataExportDir = (music: MusicXmlWithABJacket) => {
    let parentDir = "";
    switch (dirOption) {
      case MAIDATA_SUBDIR.Genre:
        parentDir =
          genreList.value.find((genre) => genre.id === music.genreId)
            ?.genreName || t("music.list.unknown");
        break;
      case MAIDATA_SUBDIR.Version:
        parentDir =
          addVersionList.value.find(
            (version) => version.id === music.addVersionId,
          )?.genreName || t("music.list.unknown");
        break;
    }

    if (parentDir) {
      parentDir = sanitizeFsSegment(parentDir, t("music.list.unknown"));
    }

    const suffix = music.id! > 1e4 && music.id! < 2e4 ? " [DX]" : "";
    const safeTitle = sanitizeFsSegment(
      music.name || t("music.list.unknown"),
      t("music.list.unknown"),
    );
    const targetDir = `${safeTitle}${suffix}`;
    return parentDir ? `${parentDir}/${targetDir}` : targetDir;
  };

  progressCurrent.value = 0;
  progressAll.value = musicList.length;
  currentProcessItem.value = "";

  setStep(STEP.ProgressDisplay);

  for (let i = 0; i < musicList.length; i++) {
    const music = musicList[i];
    const musicName = music.name || t("music.list.unknown");

    progressCurrent.value = i;
    currentProcessItem.value = musicName;

    let url = "";
    switch (action) {
      case OPTIONS.CreateNewOpt:
        url = `ExportOptApi/${music.assetDir}/${music.id}`;
        break;
      case OPTIONS.CreateNewOptCompatible:
        url = `ExportOptApi/${music.assetDir}/${music.id}?removeEvents=true`;
        break;
      case OPTIONS.CreateNewOptMa2_103:
        url = `ExportOptApi/${music.assetDir}/${music.id}?removeEvents=true&legacyFormat=true`;
        break;
      case OPTIONS.ConvertToMaidata:
        url = `ExportAsMaidataApi/${music.assetDir}/${music.id}`;
        break;
      case OPTIONS.ConvertToMaidataIgnoreVideo:
        url = `ExportAsMaidataApi/${music.assetDir}/${music.id}?ignoreVideo=true`;
        break;
    }

    try {
      const response = await fetch(getUrl(url));
      if (!response.ok || !response.body) {
        throw new Error(
          `Export request failed: ${response.status} ${response.statusText}`,
        );
      }

      const zipReader = new ZipReader(response.body);
      try {
        let hasEntryError = false;
        const entries = zipReader.getEntriesGenerator();
        for await (const entry of entries) {
          try {
            if (entry.filename.endsWith("/")) {
              continue;
            }

            let filename = entry.filename;
            if (
              action === OPTIONS.ConvertToMaidata ||
              action === OPTIONS.ConvertToMaidataIgnoreVideo
            ) {
              filename = `${getMaidataExportDir(music)}/${filename}`;
            }

            if (!entry.getData) {
              continue;
            }

            const fileHandle = await getSubDirFile(folderHandle, filename);
            const writable = await fileHandle.createWritable();
            try {
              const blob = await entry.getData(new BlobWriter());
              await writable.write(blob);
            } finally {
              await writable.close();
            }
          } catch (e) {
            hasEntryError = true;
            console.error("Failed to export zip entry", {
              musicName,
              sourceFile: entry.filename,
              error: e,
            });
          }
        }

        if (hasEntryError) {
          notify.error({
            title: t("error.exportFailed"),
            content: musicName,
          });
        }
      } finally {
        await zipReader.close();
      }
    } catch (e) {
      console.error(e);
      notify.error({
        title: t("error.exportFailed"),
        content: musicName,
      });
    }
  }

  setStep(STEP.None);
};
