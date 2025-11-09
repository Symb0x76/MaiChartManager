import { defineComponent, onMounted } from "vue";
import api from "@/client/api";
import { MusicXmlWithABJacket } from "@/client/apiGen";
import { NButton, NFlex, NSelect, NVirtualList, useDialog } from "naive-ui";
import MusicEntry from "@/components/MusicList/MusicEntry";
import { assetDirs, musicList, selectedADir, selectMusicId, updateMusicList } from "@/store/refs";
import RefreshAllButton from "@/components/RefreshAllButton";
import BatchActionButton from "@/components/MusicList/BatchActionButton";

export default defineComponent({
  props: {
    toggleMenu: { type: Function, required: true },
  },
  setup(props) {
    const dialog = useDialog();

    const setAssetsDir = async (dir: string) => {
      selectedADir.value = dir;
      selectMusicId.value = 0;
    }

    return () => (
      <NFlex vertical class="h-full" size="large">
        <NFlex>
          <NButton secondary onClick={() => props.toggleMenu()} class="min-[1440px]:hidden">
            <span class="i-ic-baseline-menu text-lg" />
          </NButton>
          <NSelect
            class="grow w-0"
            value={selectedADir.value}
            options={assetDirs.value.map(dir => ({ label: dir.dirName! + (dir.version ? ` (Ver.${dir.version})` : ''), value: dir.dirName! }))}
            onUpdateValue={setAssetsDir}
          />
          <RefreshAllButton />
          <BatchActionButton />
        </NFlex>
        <NVirtualList class="flex-1" itemSize={20 / 4 * 16} items={musicList.value}>
          {{
            default({ item }: { item: MusicXmlWithABJacket }) {
              return (
                <MusicEntry music={item} selected={selectMusicId.value === item.id} onClick={() => selectMusicId.value = item.id!} key={item.id} />
              )
            }
          }}
        </NVirtualList>
      </NFlex>
    )
  }
})
