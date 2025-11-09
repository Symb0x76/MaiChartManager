import { useDropZone } from '@vueuse/core';
import { defineComponent, PropType, ref, computed, watch, shallowRef } from 'vue';
import { startProcess as startProcessMusicImport } from '@/components/ImportCreateChartButton/ImportChartButton';

export const mainDivRef = shallowRef<HTMLDivElement>();

export default defineComponent({
  // props: {
  // },
  setup(props, { emit }) {
    const onDrop = async (files: File[] | null, e: DragEvent) => {
      e.stopPropagation();
      console.log(files);
      const items = e.dataTransfer?.items;
      if (!items?.length) return;
      const handles = await Promise.all(Array.from(items).map(item => item.getAsFileSystemHandle()));
      console.log(handles);
      if (handles.every(handle => handle instanceof FileSystemDirectoryHandle)) {
        startProcessMusicImport(handles);
      }
    }

    const { isOverDropZone } = useDropZone(mainDivRef, {
      onDrop,
      // specify the types of data to be received.
      // dataTypes: ['application/json', 'text/csv'],
      // control multi-file drop
      multiple: true,
      // whether to prevent default behavior for unhandled events
      preventDefaultForUnhandled: true,
      onOver(f, e) {
        e.stopPropagation();
        if (e.dataTransfer)
          e.dataTransfer.dropEffect = 'copy';
      },
    });

    return () => <>
    </>;
  },
});
